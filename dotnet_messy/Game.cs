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

        game = nextAction.MakeReady(input).Transform(this with {PendingActions = rest});

        if (!game.PendingActions.Any())
        {
            // phase (night or day) is over, collapse all the tags
            game = game.CollapseTags();
            // game = game with {PendingActions = game.PendingActions.Add(new DayAction(game))};
        }
        
        return game;
    }

    
    private Game CollapseTags()
    {
        return null;
        // TODO okay I think instead of this monstrosity we need a TagSet (can transform into another TagSet by copying
        // the tags (and their meta-info, that's the whole point) of the other set (union) and add the missing ones and
        // delete the unnecesary ones), then a Rule which is a transformation of one TagSet to another (maybe transformation
        // implemenation could be here instead of in the TagSet idk) and can be explicit (whole TagSet has to match) or
        // not (can be applied if the rules TagSet is only a subset of the players tags), then we also need a RuleSet which
        // is a collection of rules with an algorithm to determine the best rule to use for a given player TagSet (this
        // is covered in another comment in Program.cs I think but just explicit first (only one allowed), then the longest
        // non-explicit one. MasterTags can only be alone so a TagSet is collapsed into a single MasterTag in the end.

        /*
        var rules = new Dictionary<int, Dictionary<HashSet<Tag>, Func<IImmutableSet<Tag>, IImmutableSet<Tag>>>>
        {
            {
                1, new(HashSet<Tag>.CreateSetComparer())
                {
                    {
                        new HashSet<Tag>(new Tag[] {new Tag(Werewolf.KilledByWerewolf, null)}),
                        tags => new ImmutableHashSet<Tag>(new[]
                        {
                            MasterTags.Killed(tags.First(t => t.Identifier == Werewolf.KilledByWerewolf).CausingAction)
                        })
                    }
                }
            }
        };

        foreach (Player player in Players)
        {
            player.Tags
        }
        */
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
    public Game TagPlayer(string playerName, Tag tag) => UpdatePlayer(Players[playerName].Tag(tag));
}