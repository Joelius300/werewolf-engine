namespace WerewolfEngine;

public interface IPendingAction<out TInputRequest, in TInputSubmission> : IAction
    where TInputRequest : IInputRequest
    where TInputSubmission : IInputSubmission
{
    TInputRequest GetInputRequest();
    IReadyAction MakeReady(TInputSubmission input);
}