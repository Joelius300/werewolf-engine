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

// Guardian
// const string ProtectedByGuardian = "protected_by_guardian";
// rules.Add(new Rule(new TagSet(ProtectedByGuardian), new TagSet(), true));


// here we have a case where we see a non-explicit rule from 2 to 1. Note that in the current implementation, this
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
// of redundancies. Additionally, if all else fails, just before giving up, you could just apply one, keep going and see
// where you end up. Then backtrack and do the same for all the other colliding rules as well. If they all end up on the
// same results tagset in the end, just take it and run with it (the first one I guess). When we get to attributions and
// meta-data inside the tags, e.g. who is responsible for the killed tag on a player, this part would need to be revisited
// to make sure there is a sensible logic for determining who is or are used as responsible actions/roles for the killed
// tag. Maybe since that's a thing that will come up multiple times, have some utilities for things like creating rules
// from a tagset to another where one rule is created for all possible (r-)combinations of those tags in the first set.
// You could probably still work with the SortedList. As index use an int with a priority bit-mask (the 3 highest bytes)
// and the last byte being used to have multiple of the same priority. Then when you find the first match in the list
// keep going until the priority changes. These (up to 256) rules with the same priority, are the ones you need to check
// the collision on as described above. You could also split up 12,10,10 bits to get 4mil priorities with 1k rules each.
// Ps. since from size is sorted DESC, then to size ASC and now potentially i also ASC, it might make more sense to just
// use a normal comparer and simply subtract the from size from the maximum possible from size. Then you only have one
// DESC in an otherwise ASC "system".
// This also brings me to the idea of a CombineMetaData flag for a rule. This flag would indicate that when collapsing
// a TagSet, the meta-data of all the tags that aren't also in the To set (those can still be copied 1:1) are combined
// into a list of responsible actions, etc. and added to all new tags in the To set (those who weren't in From and thus
// couldn't just copy the meta-data). These would be used for all the killed rules. I imagine a method, that takes a set
// of all tags in the game, picks out only the non-master-tags who start with "killed", creates all combinations with
// them, creates an explicit rule for each combination that just leads to "killed" with the CombineMetaData flag set.

RuleSet ruleSet = new(rules);
TagSet tags = new(KilledByWerewolf);
Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just killed");
tags = new(KilledByWitch);
Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just killed");
tags = new(KilledByWerewolf, HealedByWitch);
Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just nothing");
