using WerewolfEngine.State;

namespace WerewolfEngine.Actions;

public abstract class BaseAction<TRole, TInputRequest, TInputResponse> : IAction<TInputRequest, TInputResponse>, IAction
    where TRole : class, IRole
    where TInputRequest : class, IInputRequest
    where TInputResponse : class, IInputResponse
{
    public Type InputRequestType => typeof(TInputRequest);
    public Type InputResponseType => typeof(TInputResponse);
    
    protected RoleAccessor<TRole> OriginRole { get; }

    protected BaseAction(RoleAccessor<TRole> originRole)
    {
        OriginRole = originRole;
    }

    public abstract TInputRequest GetInputRequest(GameState game);
    public abstract GameState Transform(GameState game, TInputResponse input);

    IInputRequest IAction.GetInputRequest(GameState game) => GetInputRequest(game);

    GameState IAction.Transform(GameState game, IInputResponse input)
    {
        if (input is TInputResponse response)
            return Transform(game, response);

        throw new ArgumentException(
            $"The input response has an incompatible type. Requires {typeof(TInputResponse).Name}", nameof(input));
    }

    public IAction ToPlainAction() => this;
}