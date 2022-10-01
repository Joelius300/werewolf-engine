using WerewolfEngine.Actions;

namespace WerewolfEngine.Werewolf;

public class WitchAction : BaseAction<WitchInputRequest, WitchInputResponse>
{
    private new string ActingPlayer => base.ActingPlayer!;

    public WitchAction(string witch) : base(witch)
    {
    }
    
    public override WitchInputRequest GetInputRequest(IGame game)
    {
        var witch = game.GetPlayer(ActingPlayer);
        var role = witch.GetRole<WitchRole>();

        return new WitchInputRequest(role.HealSpellCount, role.KillSpellCount);
    }

    public override IGame Do(IGame game, WitchInputResponse input)
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
