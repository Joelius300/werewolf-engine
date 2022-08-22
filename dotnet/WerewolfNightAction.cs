namespace WerewolfEngine;

public class WerewolfNightAction : BaseAction<WerewolfInputRequest, WerewolfInputSubmission>
{
    public WerewolfNightAction(Player responsiblePlayer, IRole responsibleRole) : base(responsiblePlayer,
        responsibleRole)
    {
    }

    public override WerewolfInputRequest GetInputRequest() => new();
    
    // TODO instead of killing the player here, only mark them with a killed_by_werewolf tag (defined by the role itself)
    public override Game Transform(Game game) => game.KillPlayer(Input.Target);
}