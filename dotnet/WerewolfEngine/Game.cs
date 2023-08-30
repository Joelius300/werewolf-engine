using System.Collections.Immutable;
using System.Diagnostics;
using WerewolfEngine.Actions;
using WerewolfEngine.Rules;
using WerewolfEngine.State;

namespace WerewolfEngine;

public class Game : IGame
{
    public GameState State { get; private set; }
    public RuleSet RuleSet { get; }

    public Game(PlayerCircle players, RuleSet ruleSet)
    {
        RuleSet = ruleSet;
        State = new GameState(
            GamePhase.Night,
            1,
            GameActionState.AwaitingActionGathering,
            Winner: null,
            players,
            CurrentAction: null,
            NextActions: ImmutableList<IAction>.Empty
        );

        State = DoActionGathering(State);
    }

    public IInputRequest GetCurrentInputRequest()
    {
        Debug.Assert((State.State != GameActionState.AwaitingInput) == (State.CurrentAction is null),
            "State != awaiting input is in sync with CurrentAction being null");

        if (State.State != GameActionState.AwaitingInput || State.CurrentAction is null)
            throw new InvalidOperationException(
                "Currently not accepting input, does not make sense to fetch input request");

        return State.CurrentAction.GetInputRequest(State);
    }

    /// Advances the core game loop until more input is requested or the game has ended.
    // here it basically is a finite state machine where the values of the GameActionState
    // are the finite states and these methods are the transitions between them.
    public void Advance(IInputResponse inputResponse)
    {
        State = DoCurrentAction(State, inputResponse);

        while (State.State != GameActionState.AwaitingInput &&
               State.State != GameActionState.GameEnded)
            Step();

        // manual stepping but that's not how you should do FSM AFAIK

        /*
        State = DoCurrentAction(State, inputResponse);
        if (State.State == GameActionState.AwaitingInput) return; // keep going

        State = DoTagCollapse(State, RuleSet);
        State = DoTagConsequences(State, RuleSet);
        State = DoWinConditionEvaluation(State);

        if (State.State == GameActionState.GameEnded) return;

        // at this point everything from the last phase has happened. It is here
        // that it's certain that the game continues (no one has won yet)
        // and that the players' states won't change anymore.

        State = DoPhaseAndRoundAdvancement(State);

        // If it was night, it's now day, basically here is the morning where
        // the GM would announce what happened at night. Since the player states
        // do not change, we can do the action gathering here and put the game
        // into the awaiting input state again already.

        State = DoActionGathering(State);

        // now the actions are gathered and input can be received again
        */
    }

    public void Step(IInputResponse? input = null)
    {
        // maybe this check should work differently. AwaitingInput could be handled entirely separate. Also use case for step outside class?
        // ties into this: instead of providing the input as parameter, have a 1-long "queue" where you can provide ("enqueue") an
        // input and the DoCurrentAction just reads and clears that queue instead of taking input on Step. Details.
        if ((input is not null) != (State.State == GameActionState.AwaitingInput))
            throw new InvalidOperationException($"State must be '{nameof(GameActionState.AwaitingInput)}' when" +
                                                "providing an input response, and you must not provide an input if it is not.");

        State = State.State switch
        {
            // these methods could be (or call) virtual "OnAwaitingInput" for example that allow hooking into.
            GameActionState.AwaitingInput => DoCurrentAction(State, input!),
            GameActionState.AwaitingTagCollapse => DoTagCollapse(State, RuleSet),
            GameActionState.AwaitingTagConsequences => DoTagConsequences(State, RuleSet),
            GameActionState.AwaitingWinConditionEvaluation => DoWinConditionEvaluation(State),
            GameActionState.AwaitingPhaseAdvancement => DoPhaseAndRoundAdvancement(State),
            GameActionState.AwaitingActionGathering => DoActionGathering(State),
            GameActionState.GameEnded => State,
            _ => throw new InvalidOperationException($"Invalid {nameof(GameActionState)} '{State.State}'")
        };
    }

    // AwaitingInput -> AwaitingInput (if more actions are ready)
    // AwaitingInput -> AwaitingTagCollapse (if no more actions are ready)
    private static GameState DoCurrentAction(GameState state, IInputResponse inputResponse)
    {
        Debug.Assert((state.State != GameActionState.AwaitingInput) == (state.CurrentAction is null),
            "State != awaiting input is in sync with CurrentAction being null");

        if (state.State != GameActionState.AwaitingInput || state.CurrentAction is null)
            throw new InvalidOperationException("Currently not accepting input, cannot submit");

        state = state.CurrentAction.Transform(state, inputResponse);

        if (state.NextActions.Any())
        {
            // in rust this could be mut here so no need to clone
            state = state with
            {
                State = GameActionState.AwaitingInput,
                CurrentAction = state.NextActions[0],
                NextActions = state.NextActions.RemoveAt(0),
            };
        }
        else
        {
            state = state with
            {
                State = GameActionState.AwaitingTagCollapse,
                CurrentAction = null
            };
        }

        return state;
    }

    // AwaitingTagCollapse -> AwaitingTagConsequences
    private static GameState DoTagCollapse(GameState state, RuleSet ruleSet)
    {
        if (state.State != GameActionState.AwaitingTagCollapse)
            throw new InvalidOperationException("Not awaiting tag collapse, cannot collapse");

        return state with
        {
            State = GameActionState.AwaitingTagConsequences,
            Players = new PlayerCircle(state.Players.Select(p => p with {Tags = ruleSet.Collapse(p.Tags)}))
        };
    }

    // AwaitingTagConsequences -> AwaitingWinConditionEvaluation
    private static GameState DoTagConsequences(GameState state, RuleSet ruleSet)
    {
        if (state.State != GameActionState.AwaitingTagConsequences)
            throw new InvalidOperationException("Not awaiting tag consequences");

        return state with
        {
            State = GameActionState.AwaitingWinConditionEvaluation,
            Players = new PlayerCircle(state.Players.Select(ruleSet.TransformAccordingToMasterTags))
        };
    }

    // AwaitingWinConditionEvaluation -> AwaitingPhaseAdvancement
    // AwaitingWinConditionEvaluation -> GameEnded
    private static GameState DoWinConditionEvaluation(GameState state)
    {
        if (state.State != GameActionState.AwaitingWinConditionEvaluation)
            throw new InvalidOperationException("Not awaiting win condition evaluation");

        if (state.Players.All(p => p.State == PlayerState.Dead))
            return state with
            {
                State = GameActionState.GameEnded,
                Winner = null
            };

        foreach (var faction in state.GetFactionsInPlay())
        {
            if (faction.HasWon(state))
            {
                // allows only one winner currently
                return state with
                {
                    State = GameActionState.GameEnded,
                    Winner = faction
                };
            }
        }

        return state with
        {
            State = GameActionState.AwaitingPhaseAdvancement,
        };
    }

    // AwaitingPhaseAdvancement -> AwaitingActionGathering
    private static GameState DoPhaseAndRoundAdvancement(GameState state)
    {
        if (state.State != GameActionState.AwaitingPhaseAdvancement)
            throw new InvalidOperationException("Not awaiting phase advancement");

        var wasDay = state.Phase == GamePhase.Day;
        return state with
        {
            State = GameActionState.AwaitingActionGathering,
            Phase = wasDay ? GamePhase.Night : GamePhase.Day,
            Round = wasDay ? state.Round + 1 : state.Round
        };
    }

    // AwaitingActionGathering -> AwaitingInput
    private GameState DoActionGathering(GameState state)
    {
        if (state.State != GameActionState.AwaitingActionGathering)
            throw new InvalidOperationException("Not awaiting action gathering");

        var actions = new List<IAction>();

        if (state.Phase == GamePhase.Day)
            actions.Add(new DayVotingAction());

        // TODO here roles need to be ordered according to some role order as stored in the game or maybe ruleset class
        // done, but maybe improve error message for roles that aren't specified in the role ordering

        // TODO Also somehow de-duplication of werewolf actions needs to be implemented. Might be possible to hard-code,
        // I don't think there are other roles like it.
        foreach (var role in state.GetRolesInPlay()
                     .Where(r => r.MightHaveAction)
                     .OrderBy(r => RuleSet.RoleOrder[r.Name]))
        {
            var action = state.Phase == GamePhase.Night ? role.GetNightAction(state) : role.GetDayAction(state);

            if (action is not null)
                actions.Add(action);
        }

        return state with
        {
            State = GameActionState.AwaitingInput,
            CurrentAction = actions.First(),
            NextActions = actions.Skip(1).ToImmutableList()
        };
    }
}