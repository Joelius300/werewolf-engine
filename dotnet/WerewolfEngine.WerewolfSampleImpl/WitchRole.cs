using WerewolfEngine.Actions;
using WerewolfEngine.State;
using WerewolfEngine.WerewolfSampleImpl.Factions;

namespace WerewolfEngine.WerewolfSampleImpl;

public record WitchRole : BaseRole<WitchRole>
{
    public const string RoleName = "witch";
    public const string KilledByWitch = "killed_by_witch";
    public const string HealedByWitch = "healed_by_witch";

    public int HealSpellCount { get; init; }
    public int KillSpellCount { get; init; }

    public WitchRole(string playerName, int healSpellCount, int killSpellCount) : base(RoleName, new VillageFaction(), playerName)
    {
        HealSpellCount = healSpellCount;
        KillSpellCount = killSpellCount;
    }

    public override IAction? GetNightAction(GameState game) => new WitchAction(Location);
    
    public record Blueprint(int InitialHealSpellCount, int InitialKillSpellCount) : IRoleBlueprint
    {
        public IRole Build(string playerName) => new WitchRole(playerName, InitialHealSpellCount, InitialKillSpellCount);
    }
}