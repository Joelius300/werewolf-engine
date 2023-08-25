using WerewolfEngine.Actions;
using WerewolfEngine.State;
using WerewolfEngine.WerewolfSampleImpl.Factions;

namespace WerewolfEngine.WerewolfSampleImpl;

public record GuardianRole : BaseRole<GuardianRole>
{
    public const string RoleName = "guardian";
    public const string ProtectedByGuardian = "protected_by_guardian";

    public string? LastProtectedPlayer { get; init; }

    public GuardianRole(string playerName) : base(RoleName, new VillageFaction(), playerName)
    {
    }

    public override IAction? GetNightAction(GameState game) => new GuardianAction(Location);

    public record Blueprint() : IRoleBlueprint
    {
        public IRole Build(string playerName) => new GuardianRole(playerName);
    }
}