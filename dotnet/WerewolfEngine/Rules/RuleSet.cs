using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using WerewolfEngine.State;

namespace WerewolfEngine.Rules;

/// <summary>
/// A set of rules defining how any tag set can be reduced to a set of master tags the
/// core engine can handle. Immutable for now.
/// </summary>
// It probably makes sense to make this mutable later on with the option of checking if a rule can be added (or if there
// is a collision and if so with which rule), as well as just adding rules after creation. Especially once utility
// functions are used to create large numbers of rules to mimic wildcard rules, you'd only want to add those that don't
// already exist (so checking for that would be another public method and could be used inside the collision check as well).
public sealed class RuleSet
{
    private readonly IDictionary<uint, ICollection<Rule>> _explicitRules;
    private readonly IDictionary<uint, ICollection<Rule>> _nonExplicitRules; // sorted by priority DESC
    private readonly int _numberOfRules;
    private readonly TagSet _potentiallyHandledTags = new(MasterTag.AllMasterTags());
    
    public IReadOnlyDictionary<string, int> RoleOrder { get; }

    public RuleSet(IEnumerable<Rule> rules, IEnumerable<string> roleOrder)
    {
        _explicitRules = new Dictionary<uint, ICollection<Rule>>();
        _nonExplicitRules = new SortedDictionary<uint, ICollection<Rule>>(Comparer<uint>.Create((x, y) => y.CompareTo(x)));
        foreach (var rule in rules)
        {
            var appropriateBuckets = rule.Explicit ? _explicitRules : _nonExplicitRules;
            uint priority = GetRulePriority(rule);
            if (appropriateBuckets.TryGetValue(priority, out var appropriateBucket))
            {
                // CollidesWith is for possibility of collision, we want to check for guaranteed collision
                Rule? collidingRule = appropriateBucket.FirstOrDefault(r => r.From == rule.From);

                if (collidingRule is not null)
                    throw new ArgumentException(
                        $"Rule collision found in {(!rule.Explicit ? "non " : "")}explicit rules for " +
                        $"priority {priority} (From: {rule.FromSize}; To: {rule.ToSize}): {collidingRule} and {rule}",
                        nameof(rules));
            }
            else
            {
                appropriateBucket = new List<Rule>();
                appropriateBuckets.Add(priority, appropriateBucket);
            }

            appropriateBucket.Add(rule);
            _potentiallyHandledTags = _potentiallyHandledTags.Union(rule.From);
        }

        _numberOfRules = _explicitRules.Values.Sum(c => c.Count) + _nonExplicitRules.Values.Sum(c => c.Count);
        if (_numberOfRules == 0) // check here instead of the top to avoid the multiple enumerations warning
            throw new ArgumentException("Must at least specify one rule.", nameof(rules));

        Rule? problematicRule = EnumerateAllRules().FirstOrDefault(r => !r.To.IsSubsetOf(_potentiallyHandledTags));
        if (problematicRule is not null)
            throw new ArgumentException(
                $"Rule {problematicRule} transforms into a set featuring the tags " +
                $"{problematicRule.To.Except(_potentiallyHandledTags)} which cannot be handled by any rule in this ruleset.",
                nameof(rules));

        RoleOrder = roleOrder.Select(KeyValuePair.Create).ToImmutableDictionary();
    }

    /// Collapses a given set of tags according to the rules defined by this ruleset. If this function returns a tagset,
    /// it is guaranteed to have been fully collapsed, otherwise it would have thrown an appropriate exception.
    public TagSet Collapse(TagSet playerTags)
    {
        if (playerTags.IsFullyCollapsed())
            return playerTags;

        if (!playerTags.IsSubsetOf(_potentiallyHandledTags))
            throw new InvalidOperationException(
                $"Tagset {playerTags} cannot be handled by this ruleset because the tags " +
                $"{playerTags.Except(_potentiallyHandledTags)} aren't present in any of the defined rules.");

        int maxIter = _numberOfRules; // if every rule is applied once and it doesn't fully collapse, it couldn't ever
        TagSet updatedTags = playerTags;
        for (int i = 0; i < maxIter; i++)
        {
            if (!TryCollapseOneStep(updatedTags, out var collapsed))
                throw new InvalidOperationException(
                    $"This ruleset as it's configured cannot collapse {playerTags} fully; " +
                    $"no matching rule found for {updatedTags}.");

            updatedTags = collapsed;
            if (updatedTags.IsFullyCollapsed())
                return updatedTags;
        }

        throw new InvalidOperationException(
            $"Even after {maxIter} iterations (number of rules), the tags could not be fully collapsed. " +
            $"Ended with {updatedTags}. Probably some sort of loop somewhere even though that shouldn't be possible.");
    }

    private bool TryCollapseOneStep(TagSet playerTags, [MaybeNullWhen(false)] out TagSet collapsed)
    {
        collapsed = CollapseByExplicitRule(playerTags);
        if (collapsed is not null)
            return true;

        collapsed = CollapseByNonExplicitRule(playerTags);

        return collapsed is not null;
    }

    private TagSet? CollapseByExplicitRule(TagSet playerTags) =>
        FindMatchingExplicitRule(playerTags)?.Collapse(playerTags);
    
    private Rule? FindMatchingExplicitRule(TagSet playerTags)
    {
        if (!_explicitRules.TryGetValue(GetExplicitRulePriority(playerTags.Count), out var explicitRuleForThatSize))
            return null;

        // we can be sure there's only one that matches thanks to the collision checks in the ctor
        return explicitRuleForThatSize.SingleOrDefault(r => r.Matches(playerTags));
    }

    /// Collapses if possible. Returns null if no rule matches. If a fatal collision occurs, the method throws instead.
    private TagSet? CollapseByNonExplicitRule(TagSet playerTags)
    {
        // first in a list sorted by priority descending
        var firstMatchingBucket = _nonExplicitRules.Values.FirstOrDefault(rules => rules.Any(r => r.Matches(playerTags)));
        if (firstMatchingBucket is null)
            return null;
        
        var allMatchingRules = firstMatchingBucket.Where(r => r.Matches(playerTags)).ToList();

        if (allMatchingRules.Count == 1) // no collision, as easy as it gets
            return allMatchingRules[0].Collapse(playerTags);

        // check if all rules would have the same result in one step (e.g. AB -> B and AC -> C both transform ABC into BC in one step)
        var allCollapsed = allMatchingRules.Select(r => r.Collapse(playerTags));
        var allCollapsedDistinct = allCollapsed.Distinct().ToList(); // TODO retain meta-data

        if (allCollapsedDistinct.Count == 1) // doesn't matter which rule we apply, they all do the same thing
            return allCollapsedDistinct[0];
        
        // if the matching rules open up multiple paths of collapse we fully evaluate/collapse each branch, then check
        // if they all result in the same collapsed end result and if so we return that. Otherwise we cannot handle the collision.
        var allFullyCollapsed = allCollapsedDistinct.ConvertAll(tags =>
        {
            try
            {
                return Collapse(tags);
            }
            catch (Exception e)
            {
                // it is up to design whether this should throw or just ignore that branch.
                throw new InvalidOperationException($"Could not fully collapse one of the branches opened by one or " +
                                                    $"more of the matching rules {string.Join(", ", allMatchingRules)}. " +
                                                    $"The branch in question starts with {playerTags} -> {tags}", e);
            }
        });
        
        var allFullyCollapsedDistinct = allFullyCollapsed.Distinct().ToList(); // TODO retain meta-data
        if (allFullyCollapsedDistinct.Count == 1) // doesn't matter which rule we apply, they all end up in the same end result
            return allFullyCollapsedDistinct[0];

        throw new InvalidOperationException($"A non-resolvable collision occured with the rules " +
                                            $"{string.Join(", ", allMatchingRules)} opening the branches " +
                                            $"{VisualizeBranches(playerTags, allCollapsedDistinct)} collapsing into " +
                                            $"multiple distinct end results: {string.Join(", ", allFullyCollapsedDistinct)}");
    }

    public Player TransformAccordingToMasterTags(Player player)
    {
        if (!player.Tags.IsFullyCollapsed())
            throw new InvalidOperationException("Cannot transform player according to master tags if their" +
                                                "tag set is not fully collapsed.");
        
        switch (player.Tags.Count)
        {
            case 0:
                return player;
            case 1:
                if (player.Tags.Single() == MasterTag.Killed)
                    return player with
                    {
                        State = PlayerState.Dead,
                        Tags = new TagSet()
                    };
                else goto default;
            default:
                throw new NotImplementedException("The fully collapsed TagSet cannot be handled by the game " +
                                                  "rules yet: " +
                                                  player.Tags);
        }
    }

    private IEnumerable<Rule> EnumerateAllRules() =>
        _explicitRules.SelectMany(kvp => kvp.Value).Concat(_nonExplicitRules.SelectMany(kvp => kvp.Value));

    private static uint GetRulePriority(Rule rule) =>
        rule.Explicit ? GetExplicitRulePriority(rule.FromSize) : GetNonExplicitRulePriority(rule.FromSize, rule.ToSize);

    private static uint GetExplicitRulePriority(int fromSize) => (uint)fromSize;

    private static uint GetNonExplicitRulePriority(int fromSize, int toSize)
    {
        if (fromSize > 0xFFFF || toSize > 0xFFFF)
            throw new ArgumentException(
                "Priority has to fit in 4 bytes so both From and To may not exceed 0xFFFF in length.");

        // the first two bytes store the from size and the last two store the "negative" of the to size.
        // this results in a 32 bit number that grows when first maximizing From and then minimizing To.
        return ((uint) fromSize << 16) | (0xFFFF - (uint) toSize);
    }

    private static string VisualizeBranches(TagSet startPoint, IEnumerable<TagSet> continuations) =>
        string.Join(", ", continuations.Select(c => $"{startPoint} -> {c}"));
}