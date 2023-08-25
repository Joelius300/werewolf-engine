using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine.WerewolfSampleImpl;

public class WitchAction : BaseAction<WitchRole, WitchInputRequest, WitchInputResponse>
{
    public WitchAction(RoleAccessor<WitchRole> originRole) : base(originRole)
    {
    }
    
    public override WitchInputRequest GetInputRequest(GameState game)
    {
        var role = game.Query(OriginRole);
        var werewolfTarget = game.Players.SingleOrDefault(p => p.Tags.Contains(WerewolfRole.KilledByWerewolves));
        return new WitchInputRequest(werewolfTarget, role.HealSpellCount, role.KillSpellCount);
    }

    public override GameState Transform(GameState game, WitchInputResponse input)
    {
        if (input.HealTargetName is not null)
        {
            game.CheckAlive(input.HealTargetName);
            game = game.TagPlayer(input.HealTargetName, WitchRole.HealedByWitch);
            game = game.UpdateRole(OriginRole, r => r with {HealSpellCount = r.HealSpellCount - 1});
        }

        if (input.KillTargetName is not null)
        {
            game.CheckAlive(input.KillTargetName);
            game = game.TagPlayer(input.KillTargetName, WitchRole.KilledByWitch);
            game = game.UpdateRole(OriginRole, r => r with {KillSpellCount = r.KillSpellCount - 1});
        }

        return game;
    }
}