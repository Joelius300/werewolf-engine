using System.Collections.Immutable;

Console.WriteLine("Hello, World!");

record Game(IReadOnlyList<Player> Players)
{
    public IReadOnlyList<IPendingAction<IInputRequest, IInputSubmission>> PendingActions { get; init; }

    public Game SubmitInput(IInputSubmission input)
    {
        Game game = this;
        if (!PendingActions.Any())
        {
            game = BuildActionQueue(game);
        }

        var (nextAction, rest) = PendingActions.Pop();

        game = nextAction.MakeReady(input).Transform(this);
        return game with { PendingActions = rest };
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

        return game with {PendingActions = actions};
    }
}

public static class ReadOnlyListExtensions
{
    public static (T item, IReadOnlyList<T> rest) Pop<T>(this IReadOnlyList<T> collection)
    {
        return (collection[0], collection.Skip(1).ToImmutableList());
    }
}

record Player(string Name, PlayerState State, IReadOnlyList<IRole> Roles)
{
    
}

enum PlayerState
{
    Alive,
    Dead
}

interface IRole
{
    bool RegisterNightAction(Game game, IList<IPendingAction<IInputRequest, IInputSubmission>> actions);
}

interface IPendingAction<out TInputRequest, in TInputSubmission>
    where TInputRequest : IInputRequest
    where TInputSubmission : IInputSubmission
{
    TInputRequest GetInputRequest();
    IReadyAction MakeReady(TInputSubmission input);
}

interface IReadyAction
{
    Game Transform(Game game);
}

interface IInputRequest {}
interface IInputSubmission {}