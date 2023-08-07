using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine.WerewolfSampleImpl;

public class WitchAction : BaseAction<WitchInputRequest, WitchInputResponse>
{
    public override WitchInputRequest GetInputRequest(GameState game)
    {
        var witch = game.GetPlayer(ActingPlayer);
        var role = witch.GetRole<WitchRole>();

        return new WitchInputRequest(role.HealSpellCount, role.KillSpellCount);
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