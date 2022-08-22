using System.Collections.Immutable;

namespace WerewolfEngine;

public record Game(PlayerCircle Players)
{
    public IImmutableList<IPendingAction<IInputRequest, IInputSubmission>> PendingActions { get; init; } =
        ImmutableArray<IPendingAction<IInputRequest, IInputSubmission>>.Empty;
    public IImmutableList<IReadyAction> PastActions { get; init; } = ImmutableArray<IReadyAction>.Empty;

    public Game SubmitInput(IInputSubmission input)
    {
        Game game = this;
        if (!PendingActions.Any())
        {
            game = BuildActionQueue(game);
        }

        var (nextAction, rest) = PendingActions.Pop();

        game = nextAction.MakeReady(input).Transform(this);
        return game with {PendingActions = rest};
    }

    private static Game BuildActionQueue(Game game)
    {
        var actions = new List<IPendingAction<IInputRequest, IInputSubmission>>();
        foreach (var player in game.Players)
        {
            foreach (var role in player.Roles)
            {
                role.RegisterNightAction(game, actions);
            }
        }

        return game with {PendingActions = actions.ToImmutableList()};
    }

    public Game UpdatePlayer(Player updatedPlayer) => this with {Players = Players.UpdatePlayer(updatedPlayer)};
    public Game KillPlayer(string name) => UpdatePlayer(Players[name].Kill());
}