namespace WerewolfEngine;

public interface IRole
{
    bool RegisterNightAction(Game game, IList<IPendingAction<IInputRequest, IInputSubmission>> actions);
}