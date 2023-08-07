using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine.WerewolfSampleImpl;

public class WitchAction : BaseAction<WitchRole, WitchInputRequest, WitchInputResponse>
{
    public WitchAction(WitchRole originRole) : base(originRole)
    {
    }
    
    public override WitchInputRequest GetInputRequest(GameState game)
    {
        // TODO: The HealSpellCount and the KillSpellCount MUST be taken from the
        // passed game state. Probably by making OriginRole just an accessor that allows
        // getting the correct role from the new GameState.
        return new WitchInputRequest(OriginRole.HealSpellCount, OriginRole.KillSpellCount);
    }

    public override GameState Transform(GameState game, WitchInputResponse input)
    {
        if (input.HealTargetName is not null)
        {
            game = game.UpdatePlayer(game.GetPlayer(input.HealTargetName).Tag(WitchRole.HealedByWitch));
            // or
            game = game.UpdatePlayer(input.HealTargetName, p => p.Tag(WitchRole.HealedByWitch));
            // or
            game = game.TagPlayer(input.HealTargetName, WitchRole.HealedByWitch);

            game.UpdatePlayer(ActingPlayer, p => p.UpdateRole<WitchRole>(r => r.UpdateHealSpellCount(-1)));
        }

        if (input.KillTargetName is not null)
        {
            // same here
        }

        return game;
    }
}
