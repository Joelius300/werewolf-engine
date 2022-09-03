namespace WerewolfEngine;

public abstract class BaseAction<TInputRequest, TInputSubmission> : IPendingAction<TInputRequest, TInputSubmission>, // IPendingAction<IInputRequest, IInputSubmission>,
    IReadyAction
    where TInputRequest : IInputRequest
    where TInputSubmission : class, IInputSubmission
{
    public Player ResponsiblePlayer { get; }
    public IRole ResponsibleRole { get; }
    protected TInputSubmission? Input { get; private set; }

    protected BaseAction(Player responsiblePlayer, IRole responsibleRole)
    {
        ResponsiblePlayer = responsiblePlayer;
        ResponsibleRole = responsibleRole;
    }

    protected abstract Game Transform(Game game);

    Game IReadyAction.Transform(Game game)
    {
        if (Input is null)
            throw new InvalidOperationException("Action is not ready yet.");

        Game transformed = Transform(game);
        return transformed with {PastActions = transformed.PastActions.Add(this)};
    }

    public abstract TInputRequest GetInputRequest();
    
    /*
    IInputRequest IPendingAction<IInputRequest, IInputSubmission>.GetInputRequest() => GetInputRequest();
    IReadyAction IPendingAction<IInputRequest, IInputSubmission>.MakeReady(IInputSubmission input) => MakeReady((input as TInputSubmission)!);
    */

    public virtual IReadyAction MakeReady(TInputSubmission input)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
        return this;
    }

    protected Tag CreateTag(string tag) => new(tag, this);
}