namespace WerewolfEngine.Actions;

public abstract class BaseAction<TInputRequest, TInputResponse> : IAction<TInputRequest, TInputResponse>, IAction
    where TInputRequest : IInputRequest
    where TInputResponse : IInputResponse
{
    public string? ActingPlayer { get; }

    public virtual IReadOnlyCollection<Type> BeforeActionDependencies { get; }

    protected BaseAction(string? actingPlayer, IEnumerable<Type>? beforeActionDependencies = null)
    {
        ActingPlayer = actingPlayer;
        BeforeActionDependencies = beforeActionDependencies?.ToArray() ?? Array.Empty<Type>();
    }

    public abstract TInputRequest GetInputRequest(IGame game);
    public abstract IGame Do(IGame game, TInputResponse input);

    IInputRequest IAction.GetInputRequest(IGame game) => GetInputRequest(game);

    IGame IAction.Do(IGame game, IInputResponse input)
    {
        if (input is TInputResponse response)
            return Do(game, response);

        throw new ArgumentException(
            $"The input response has an incompatible type. Requires {typeof(TInputResponse).Name}", nameof(input));
    }
}