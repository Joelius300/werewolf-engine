using WerewolfEngine;

List<Rule> rules = new();

// Werewolf
const string KilledByWerewolf = "killed_by_werewolf";
rules.Add(new Rule(new TagSet(KilledByWerewolf), new TagSet(MasterTag.Killed), true));

// Witch
const string HealedByWitch = "healed_by_witch";
const string KilledByWitch = "killed_by_witch";
rules.Add(new Rule(new TagSet(HealedByWitch), new TagSet(), true));
rules.Add(new Rule(new TagSet(KilledByWitch), new TagSet(MasterTag.Killed), true));
rules.Add(new Rule(new TagSet(KilledByWerewolf, HealedByWitch), new TagSet(HealedByWitch), false));
// no Rule for killed_by_werewolf and killed_by_witch since that can't happen in game (well that's up to the GM..)
// in the same way healed_by_witch and killed_by_witch don't work together either (define either error or resolve)
// also here we have a case where we see a non-explicit rule from 2 to 1. Note that in the current implementation, this
// would block all other non-explicit rules 2->1 from being added. This might be too limiting because then you can't
// define healed + killed for other roles in the same way. One way to solve this would be to not block these collisions
// and instead only do the recovery when there are more than one rule of the same priority that match. In that case you
// could for example try to find an order of applying all (e.g. try one, check if the next one also matches, check if
// the next also matches, etc. and backtrack until you find an order that manages to apply all of them or throws if not).
// Imagine the case of killed_by_werewolf, killed_by_witch and protected_by_guardian. You could create two rules for the
// protection namely killed_by_werewolf + protected_by_guardian -> protected_by_guardian and 
// killed_by_witch + protected_by_guardian -> protected_by_guardian. The algorithm would then find that first applying
// rule one would reduce it to killed_by_witch + protected_by_guardian and that still matches the second rule which could
// then be applied as well. I think that would allow for much more flexibility when defining the rules and prevent a lot
// of redundancies.

RuleSet ruleSet = new(rules);
TagSet tags = new(KilledByWerewolf);
Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just killed");
tags = new(KilledByWitch);
Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just killed");
tags = new(KilledByWerewolf, HealedByWitch);
Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just nothing");
