using WerewolfEngine.State;

namespace WerewolfEngine.WerewolfSampleImpl.Factions;

public class VillageFaction : BaseFaction
{
    private const string FactionName = "village";

    public VillageFaction() : base(FactionName, isWerewolfFaction: false)
    {
    }

    // all werewolves must be dead
    public override bool HasWon(GameState gameState) =>
        gameState.Players.Where(p => p.ActiveFaction.IsWerewolfFaction).All(p => p.State == PlayerState.Dead);
}