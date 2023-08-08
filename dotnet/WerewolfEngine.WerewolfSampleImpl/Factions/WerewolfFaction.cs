using WerewolfEngine.State;

namespace WerewolfEngine.WerewolfSampleImpl.Factions;

public class WerewolfFaction : BaseFaction
{
    private const string FactionName = "werewolves";

    public WerewolfFaction() : base(FactionName, isWerewolfFaction: true)
    {
    }

    // all living players must be werewolves
    public override bool HasWon(GameState gameState) =>
        gameState.Players.Where(p => p.State == PlayerState.Alive).All(p => p.ActiveFaction.IsWerewolfFaction);
}