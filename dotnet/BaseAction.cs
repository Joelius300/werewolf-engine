namespace WerewolfEngine;

public abstract class BaseAction<TInputRequest, TInputSubmission> : IPendingAction<TInputRequest, TInputSubmission>,
    IReadyAction
    where TInputRequest : IInputRequest
    where TInputSubmission : IInputSubmission
{
    public Player ResponsiblePlayer { get; }
    public IRole ResponsibleRole { get; }
    protected TInputSubmission Input { get; set; }

    protected BaseAction(Player responsiblePlayer, IRole responsibleRole)
    {
        ResponsiblePlayer = responsiblePlayer;
        ResponsibleRole = responsibleRole;
    }

    public abstract Game Transform(Game game);

    Game IReadyAction.Transform(Game game)
    {
        if (Input is null)
            throw new InvalidOperationException("Action is not ready yet.");

        Game transformed = Transform(game);
        return transformed with {PastActions = transformed.PastActions.Add(this)};
    }

    public abstract TInputRequest GetInputRequest();

    public virtual IReadyAction MakeReady(TInputSubmission input)
    {
        Input = input;
        return this;
    }
}