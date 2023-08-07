using WerewolfEngine.State;

namespace WerewolfEngine.Actions;

public abstract class BaseAction<TInputRequest, TInputResponse> : IAction<TInputRequest, TInputResponse>, IAction
    where TInputRequest : IInputRequest
    where TInputResponse : IInputResponse
{
    public Type InputRequestType => typeof(TInputRequest);
    public Type InputResponseType => typeof(TInputResponse);
    
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