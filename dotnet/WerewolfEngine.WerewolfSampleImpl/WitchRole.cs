using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine.WerewolfSampleImpl;

public class WitchRole : BaseRole<WitchRole>
{
    private const string RoleName = "witch";
    public const string KilledByWitch = "killed_by_witch";
    public const string HealedByWitch = "healed_by_witch";

    public int HealSpellCount { get; init; }
    public int KillSpellCount { get; init; }

    public WitchRole(string playerName, int healSpellCount, int killSpellCount) : base(RoleName, playerName)
    {
        HealSpellCount = healSpellCount;
        KillSpellCount = killSpellCount;
    }

    public override IAction? GetNightAction(GameState game) => new WitchAction(Location);
}