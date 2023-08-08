using WerewolfEngine.Actions;
using WerewolfEngine.State;
using WerewolfEngine.WerewolfSampleImpl.Factions;

namespace WerewolfEngine.WerewolfSampleImpl;

public record VillagerRole : BaseRole<VillagerRole>
{
    public const string RoleName = "villager";
    
    public VillagerRole(string playerName) : base(RoleName, new VillageFaction(), playerName)
    {
    }

    public override bool MightHaveAction => false;
    public override IAction? GetNightAction(GameState game) => null;

    public record Blueprint : IRoleBlueprint
    {
        public IRole Build(string playerName) => new VillagerRole(playerName);
    }
}