using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine.WerewolfSampleImpl;

public record WitchRole(int HealSpellCount, int KillSpellCount) : IRole
{
    public const string KilledByWitch = "killed_by_witch";
    public const string HealedByWitch = "healed_by_witch";

    public string Name => "witch";

    public IAction? GetNightAction(GameState game) => new WitchAction(this);
}