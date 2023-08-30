using WerewolfEngine;
using WerewolfEngine.Rules;
using WerewolfEngine.State;
using WerewolfEngine.WerewolfSampleImpl;

namespace Playground;

public class Villager_Werewolf_Witch_UI_Playground : PlayGround
{
    public override void Play()
    {
        var players = new PlayerCircle(new[]
        {
            new Player("P1", new VillagerRole.Blueprint()),
            new Player("P2", new WitchRole.Blueprint(1, 1)),
            new Player("P3", new WerewolfRole.Blueprint()),
            new Player("P4", new VillagerRole.Blueprint()),
        });

        var rules = new RuleSet(new[]
            {
                // Werewolf
                new Rule(new TagSet(WerewolfRole.KilledByWerewolves), new TagSet(MasterTag.Killed), true),
                // Witch
                new Rule(new TagSet(WitchRole.HealedByWitch), new TagSet(), true),
                new Rule(new TagSet(WitchRole.KilledByWitch), new TagSet(MasterTag.Killed), true),
                new Rule(new TagSet(WerewolfRole.KilledByWerewolves, WitchRole.HealedByWitch),
                    new TagSet(WitchRole.HealedByWitch), false),
                // no Rule for killed_by_werewolf and killed_by_witch since that can't happen in game (well that's up to the GM..)
                // in the same way healed_by_witch and killed_by_witch don't work together either (define either error or resolve)
                // -- Day
                // Village / Environmental / God-Role
                new Rule(new TagSet(GodRole.KilledByVillage), new TagSet(MasterTag.Killed), true),
            },
            new[]
            {
                WerewolfRole.RoleName,
                WitchRole.RoleName
            });

        var game = new Game(players, rules);
        var ui = new WerewolfUI(game);
        ui.Run();
    }
}