using System.Diagnostics;
using WerewolfEngine;
using WerewolfEngine.Rules;
using WerewolfEngine.State;
using WerewolfEngine.WerewolfSampleImpl;

namespace Playground;

public class Villager_Werewolf_Witch_Guardian_Playground : PlayGround
{
    public override void Play()
    {
        var players = new PlayerCircle(new[]
        {
            new Player("P1-villager", new VillagerRole.Blueprint()),
            new Player("P2-witch", new WitchRole.Blueprint(1, 1)),
            new Player("P3-werewolf", new WerewolfRole.Blueprint()),
            new Player("P4-guardian", new GuardianRole.Blueprint()),
        });

        // For building a ruleset, it might make sense to have a format where each role has a section with self contained rules
        // like KilledByWerewolves -> Killed or HealedByWitch -> {}
        // then have sections for interactions like Werewolf + Witch, Werewolf + Guardian, Witch + Guardian, etc.
        // The program could then combine all the rules for all the roles in play and their combinations.
        // These rules should live outside of the roles because the combinations shouldn't live in only one, nor in both.
        var rules = new RuleSet(new[]
            {
                // -- Night
                // Werewolf
                new Rule(new TagSet(WerewolfRole.KilledByWerewolves), new TagSet(MasterTag.Killed), true),
                // Witch
                new Rule(new TagSet(WitchRole.HealedByWitch), new TagSet(), true),
                new Rule(new TagSet(WitchRole.KilledByWitch), new TagSet(MasterTag.Killed), true),
                new Rule(new TagSet(WerewolfRole.KilledByWerewolves, WitchRole.HealedByWitch),
                    new TagSet(WitchRole.HealedByWitch), false),
                // no Rule for killed_by_werewolf and killed_by_witch since that can't happen in game (well that's up to the GM..)
                // in the same way healed_by_witch and killed_by_witch don't work together either (define either error or resolve)
                // Guardian
                new Rule(new TagSet(GuardianRole.ProtectedByGuardian), new TagSet(), true),
                new Rule(new TagSet(WitchRole.HealedByWitch, GuardianRole.ProtectedByGuardian), new TagSet(), true),
                new Rule(new TagSet(WerewolfRole.KilledByWerewolves, GuardianRole.ProtectedByGuardian),
                    new TagSet(GuardianRole.ProtectedByGuardian), false),
                new Rule(new TagSet(WitchRole.KilledByWitch, GuardianRole.ProtectedByGuardian),
                    new TagSet(GuardianRole.ProtectedByGuardian), false),
                // -- Day
                // Village / Environmental / God-Role
                new Rule(new TagSet(GodRole.KilledByVillage), new TagSet(MasterTag.Killed), true),
            },
            new[]
            {
                GuardianRole.RoleName,
                WerewolfRole.RoleName,
                WitchRole.RoleName
            });

        var game = new Game(players, rules);

        // Scripted actions to simulate example a from concept

        Console.WriteLine("Initial game state");
        Console.WriteLine(Serialize(game.State));
        Console.WriteLine();

        PrintRequest(game.GetCurrentInputRequest());

        SubmitInput(game, new GuardianInputResponse("P1-villager"));

        PrintRequest(game.GetCurrentInputRequest());

        SubmitInput(game, new WerewolfInputResponse("P1-villager"));

        PrintRequest(game.GetCurrentInputRequest());

        SubmitInput(game, new WitchInputResponse(null, null));

        Console.WriteLine("End of night");
        // Console.WriteLine(Serialize(game.State));

        PrintRequest(game.GetCurrentInputRequest());

        SubmitInput(game, new DayVotingInputResponse("P1-villager"));

        Debug.Assert(!game.State.IsAlive("P1-villager"));

        Console.WriteLine("End of day");

        PrintRequest(game.GetCurrentInputRequest());

        SubmitInput(game, new GuardianInputResponse("P2-witch"));

        Debug.Assert(game.State.HasTag("P2-witch", GuardianRole.ProtectedByGuardian));

        PrintRequest(game.GetCurrentInputRequest());

        SubmitInput(game, new WerewolfInputResponse("P4-guardian"));

        Debug.Assert(game.State.HasTag("P4-guardian", WerewolfRole.KilledByWerewolves));

        PrintRequest(game.GetCurrentInputRequest());

        SubmitInput(game, new WitchInputResponse("P4-guardian", "P3-werewolf"));

        Debug.Assert(game.State.State == GameActionState.GameEnded);

        Console.WriteLine("End of night and game");
    }
}