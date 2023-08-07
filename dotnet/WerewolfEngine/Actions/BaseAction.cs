using WerewolfEngine.State;

namespace WerewolfEngine.Actions;

public abstract class BaseAction<TRole, TInputRequest, TInputResponse> : IAction<TInputRequest, TInputResponse>, IAction
    where TRole : IRole
    where TInputRequest : IInputRequest
    where TInputResponse : IInputResponse
{
    public Type InputRequestType => typeof(TInputRequest);
    public Type InputResponseType => typeof(TInputResponse);
    
    // IMPORTANT: It is crucial that this role reference ALWAYS POINTS TO A ROLE INSIDE THE SAME GAME STATE AS
    // THIS ACTION IS IN. With the immutability it might not matter but then the issue will be to identify the
    // same role in the new game state, in which case references like this should not be stored but instead
    // XPath like identifiers that allow querying the corresponding role/faction whatever in any other game state.
    protected TRole OriginRole { get; }

    protected BaseAction(TRole originRole)
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