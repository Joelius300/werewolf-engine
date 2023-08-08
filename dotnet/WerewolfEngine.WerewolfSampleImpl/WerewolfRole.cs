using WerewolfEngine.Actions;
using WerewolfEngine.State;
using WerewolfEngine.WerewolfSampleImpl.Factions;

namespace WerewolfEngine.WerewolfSampleImpl;

public record WerewolfRole : BaseRole<WerewolfRole>
{
    public const string RoleName = "werewolf";
    public const string KilledByWerewolves = "killed_by_werewolves";
    
    public WerewolfRole(string playerName) : base(RoleName, new WerewolfFaction(), playerName)
    {
    }

    public override IAction? GetNightAction(GameState game) => new WerewolfAction(Location);
    
    public record Blueprint : IRoleBlueprint
    {
        public IRole Build(string playerName) => new WerewolfRole(playerName);
    }
}