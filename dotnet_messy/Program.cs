using WerewolfEngine;

const string YoMam = "dini mam";

Rule rule = new(new(TemplateTag(Werewolf.KilledByWerewolf), TemplateTag(YoMam)),
 new(TemplateTag(Werewolf.KilledByWerewolf)), Explicit: true);

TagSet playerTags = new(new(Werewolf.KilledByWerewolf, new WerewolfNightAction(null!, null!)),
 new(YoMam, new WerewolfNightAction(null!, null!)));

Console.WriteLine($"Rule matches: {rule.Matches(playerTags)}");

Console.WriteLine($"Tags before collapse: {playerTags}");
Console.WriteLine($"From: {rule.From} / To: {rule.To}");

var newTags = rule.Collapse(playerTags);
Console.WriteLine($"Collapsed Tags: {newTags}");
Console.WriteLine($"Tags after collapse (should be the same still): {playerTags}");
Console.WriteLine($"From: {rule.From} / To: {rule.To}");



Tag TemplateTag(string tagId) => new(tagId, null);
// Just FYI, "with" creates a shallow copy!

// Note: These transformations should happen after all the actions are made ready.
/* It should be like some kind of collapse. First all the inputs are collected in an order that makes sense
 * but isn't critical. With every input the action is made ready which also validates the input. Then when all
 * actions are ready, they are collapsed in an order that makes sense (guardian before werewolves before witch, etc)
 * It would make sense for this to be the same order that the inputs are requested in so they can be put in a queue.
 * For this to work every player has to have a list of modifiers to keep track of things like guardians blessing,
 * werewolves target, etc.
 * Alternatively, every input could result in an immediate but non-destructive action like tagging a player as the
 * werewolves target. Then after all the tagging, etc. is done, each player collapses their state according to the
 * modifiers/tags they currently have so for example a player dies if they have the "werewolves target" but not if
 * they also have the "guardians blessing", "witches potion" and if they have the "urwolfs curse" they even get a new
 * role, etc, etc. This might also work with more complex things like the lucky bastard which would immediately transform
 * the game to a state where the werewolves target was removed from one player and added to one of it's neighbours.
 * Right now this sounds like a smart idea, not sure whether it'll work as well as I'm hoping tho.
 * Another thing. For this to be extendable without having to edit potentially many other classes when adding a new
 * character so instead of the Player collapsing the state according to the tags (which he'd have to know) the roles
 * which define (and register) the tags should also define how their state collapses. Then there would probably need
 * to be a second set of tags which is predefined like protected (potentially different kinds of protection strengths),
 * killed, etc. and these are then universally collapsed by the Player because they are so generic that it doesn't need
 * to be updated everytime a new more specific tag is added. So now my idea is as follows:
 * - werewolves add "werewolf target" tag
 * - witch adds "healed" tag
 * - night ends, all tags are distributed
 * - werewolves collapse werewolf_target to killed_from_normal_werewolves
 * - witch collapses healed to protected_from_normal_werewolves
 * - player sees the two tags and determines that the killed tag is voided by the protected tag
 *
 * I'm really not sure if that'll work without a mess...o
 * Maybe some sort of system with combinations and priorities would work? So you have this list of tags for a player
 * like protected_by_guardian, attacked_by_werewolf, healed_by_witch, whatever and then you go through a list of combinations
 * like protected + attacked = nothing, attacked + healed = nothing, attacked = killed, etc.
 * and you choose the one with the highest priority (might correlate with the number of tags involved but not sure)
 * which in this case would be protected + attacked and then these two tags are consumed. Afterwards, there's only
 * healed left and that has no impact (not in the combinations list) so it's simply discarded. This would also work
 * for more complex things like turning into a werewolf because then you might have:
 * protected, attacked_by_werewolf, turned_into_werewolf, healed  and the highest priority combo is attacked_by_werewolf
 * + turned_into_werewolf so then the other two get discarded. Going even further, if there was an
 * attacked_by_white_werewolf but no protected tag so: attacked_by_werewolf, turned_into_werewolf, healed, killed_by_white_werewolf
 * in this case the white werewolf one would alone take priority over all the other ones and kill the person. But now
 * make it spicy by adding the protected again: protected, attacked_by_werewolf, turned_into_werewolf, healed, killed_by_white_werewolf
 * now the killed_by_white_werewolf + protected is a combo that goes into nothing and leaves behind protected and we're back to
 * being turned into a werewolf.
 * This whole paragraph has lead me to the idea of reducers. You would write reducers for certain tag states like
 * killed_by_white_werewolf + protected -> protected
 * In order for this system to work, be any joy to use at all and be predictable you'd need a few things
 * - first match any exclusive rules so rules that cover the entire state of a person e.g.
 *   killed_by_werewolf + healed (and nothing else) -> nothing
 * - when no more exclusive rules match, try to match a non-exclusive rule like killed_by_white_werewolf + protected -> protected
 *   or protected + healed -> protected which would both reduce the number of tags by one, then you can go back to try for exclusive rules
 *   My gut feeling tells me that these would be the rules that need to be adjusted the most when adding new roles so
 *   it's better to write more exclusive rules but avoid just mindlessly adding brute force combinations when two tags alone would seal the deal.
 * - good testing. for every new role added, you need to test what changes for the combinations. To make this easier
 *   or potentially even bearable, you'll probably need some sort of helper application which can generate all the
 *   combinations of possible tags for a given set of roles and check if all the tag combinations are covered by the
 *   rules. If not, alert. This alert would also need to be implemented in the app later on somehow otherwise you
 *   could start a game with certain rules undefined so at the very least the app should calculate the combinations
 *   which aren't covered by the rules and list them before the game. During the game 
 * This would also allow you to be very explicit about edge cases which is one of the main points of interest for this
 * engine, to detect and correctly handle edge cases a human might forget. Luckily, those are usually only a handful of
 * tag combinations so they can be explicitly handled. Another great thing about this is actually how dynamic it is;
 * you could easily describe all the rules in text like this (with an appropriate syntax of course) and even build
 * a rule creation and adjusting dialog on top of that.
 * This is also the point in the story where finite state machines come back into play. This sounds oddly familiar
 * like those combinations are just inputs that trigger/enable a certain transition. But the states are different.
 * A players state is being dead or alive. This alone wouldn't warrant a FSM and if we add the other state a player
 * has which is their roles, it gets complicated fast because a player can have multiple roles in all kinds of combinations.
 * So I think this might work.
 * - Roles are behaviour which alter a person's or multiple people's list of tags.
 *   This needs to be smart with which kinds of tags get distributed. E.g. a killed_by_association_because_at_whores_house
 *   of some kind is almost certainly necessary because but there may be multiple ways to do it. Either you're very
 *   specific like killed_by_werewolf, killed_by_witch, killed_by_white_werewolf, etc. and all of those with a
 *   "though association because at whores house". Or you could have some sort of parameter system like
 *   action#by so you could write killed#werewolf + healed#witch -> nothing, which would also allow for #any when resolving a kill for example.
 *   Also you need to do some parts of the logic already when assigning tags for example the lucky bastard manipulates
 *   tags randomly according to his rules and that's before any reduction is done. Win conditions also need to be coded.
 * - A few tags need to be generalized and fixed like killed, silenced, role_added, role_removed. Only these "master" tags are
 *   allowed to remain after tag reduction through rules occurs.
 *
 * Other notes:
 * - lover is a role? like other roles it determines the faction of the player and like the bear it doesn't have a
 *   night action but is important to know for the engine
 * - mafioso is probably just a role too right?
 * - day actions are a whole new thing that you can worry about later
 * - there needs to be a mechanism for event actions like killing someone as they die which might be hard because
 *   you can only be certain someone died after all the states have collapsed. Also in the case of the rusty knight
 *   the next werewolf to his left dies _the next night_ so there would need to be a way to push to the pending actions
 *   queue for the next night during the current one.
 * - there also needs to be a mechanism for jump in actions like the great-werewolf but for starters you can just
 *   create an action with a request that's just yes or no.
 *
 * Frs Werwöuflä züg zuäsätzläch no Effects wobi ei effect Silenced (Oger) chönnt si. I Zuäkunft ä angerä de Drunk.
 * Effects si persistent im gägäsatz zu tags und müässä explizit entfernt wärdä.
 * Wi o Master Tags si Effects Grundsätzlächi Sachä fr Spiufunktionalität u müässä haut fix programmiert wärdä
 * Auternativ chönnt mä o zwöi sachä ha wöu zmingst fr diä App bruchts ke Logik fr silenced aso d Tag abstimmigä si
 * ja nid ir App, es wär meh ä Info aber wes so weni si chamä sä o code würdi bhouptä.
 * Vergiss dä string centrisch approach viläch wider chli, ömu am Afang.
 * Wedä ä schöni Api machsch chamä o guät mit chli typesafety arbeitä bim Tag und Effekt registriere, etc.
 * Wichtig isch aber o dasdä nid uf Types ufbousch ussert es chönntä enums si.
 * Mit däm meini dasdä nid type/interface tests mit "is" machsch wöu Rust cha das nid. MasterTags und Effects chöi widerum Enums si.
 * Zu dr queue fr diä Nacht bruchsch ono ä Queue womä fr Nacht N cha actions dri tuä wo jedi nacht z N abägeit und wes uf 0 isch
 * (oder si blibt fix u mä luägt uf d Nacht vom Game wo immer ufä geit) denn wird si id Nacht queue inä ta.
 * Apropos Nachtqueue, dr augorithnus fr nä reihäfoug z fingä sött äuä so irgendwiä si oder das wär ömu ä idee:
 * jedi action definiert actions wo vorhär bzw. nächär müässä passiere, dependencies quasi.
 * När geit mä dürä und suächt aui diä wo ke vorhär dependency hei und wäut einä, när suächt mä aui wo mit dr aktuellä queue
 * no chönntä cho und wäut wider eis bli bla blu. Wemä uf eis trifft wo nümm ine cha denn backtrackt mä.
 * Glichzitig darfmä natürläch numä serigi näh wo keni "nächär" beziehig vorä angärä noni enqueutä Action si.
 * Wemä bis am Schluss ke müglächä wäg fingt schisster säch haut ii.
 */

// TODO it'll be necessary to have a way of matching for a certain tag by id and some other predicate otherwise
// there will be many almost redundant tag definitions e.g. for being killed by different roles (may be ok to start with)