namespace WerewolfEngine;

/// <summary>
/// A set of rules defining how any tag set can be iteratively reduced to a set of master tags the
/// core engine can handle. Immutable for now.
/// </summary>
// It probably makes sense to make this mutable later on with the option of checking if a rule can be added (or if there
// is a collision and if so with which rule), as well as just adding rules after creation. Especially once utility
// functions are used to create large numbers of rules to mimic wildcard rules, you'd only want to add those that don't
// already exist (so checking for that would be another public method and could be used inside the collision check as well).
/* Maximum number of rules for n different tags is as follows:
 * (2^(n+1) + (n-1)(n+4)) / 2
 *
 * which is just the sum of the number of explicit rules and the number of non-explicit rules.
 * This function grows similarly to 2^n and for 10 unique tags, we already have over 1000 possible rules we could write.
 * I think that should easily be enough to correctly model the game of werewolf with my rules even though it might
 * require some helper methods to create wild-card like rules or something like that but we'll cross that bridge when we
 * get to it.
 *
 * Number of explicit rules:
 * 2^n - 1
 *
 * Reasoning:
 * It's the sum of all permutations of all the possible From sizes. This is almost the size of the power set
 * (all subsets including itself and the empty set) with the only difference that we don't want the empty set
 * because you cannot have a 0 From size. So we just take one from the definition of the number of elements in the power
 * set (SUM k from 0 to n of (n choose k) = 2^n) and get 2^n - 1
 * 
 *
 * Number of non-explicit rules:
 * n(n+1)/2 + (n-1)
 *
 * Reasoning: For each size of From (n down to 1) you can have one size for To (From size down to 0 = From size + 1)
 * which results in the series: (n+1) + n + (n-1) + (n-2) + ... + 2
 * now we just need to take into account that there cannot be a rule that transforms a tagset of size n to another of size
 * n because that would have to be the same tagset which is forbidden so the whole series becomes:
 * n + n + (n-1) + (n-2) + ... + 2 = n + n(n+1)/2 - 1
 */
public sealed class RuleSet
{
    private readonly IDictionary<int, ICollection<Rule>> _explicitRules;
    private readonly SortedList<int, Rule> _nonExplicitRules;
    private readonly int _numberOfRules;
    private readonly TagSet _potentiallyHandledTags = new(MasterTag.AllMasterTags());

    public RuleSet(IEnumerable<Rule> rules)
    {
        _explicitRules = new Dictionary<int, ICollection<Rule>>();
        _nonExplicitRules = new SortedList<int, Rule>(Comparer<int>.Create((x, y) => y.CompareTo(x))); // SORT DESC
        foreach (var rule in rules)
        {
            if (rule.Explicit)
            {
                if (!_explicitRules.TryGetValue(rule.FromSize, out var explicitRules))
                {
                    explicitRules = new List<Rule>();
                    _explicitRules.Add(rule.FromSize, explicitRules);
                }
                else
                {
                    Rule? collidingRule = explicitRules.FirstOrDefault(r => r.CollidesWith(rule));
                    if (collidingRule is not null)
                        throw new ArgumentException(
                            $"Rule collision found in {rule.FromSize} sized explicit rules: {collidingRule} and {rule}",
                            nameof(rules));
                }
                
                explicitRules.Add(rule);
                _potentiallyHandledTags = _potentiallyHandledTags.Union(rule.From);
            }
            else
            {
                // we first want to maximize the from size, then minimize the rule size
                int priority = rule.FromSize * 1000 - rule.ToSize;
                if (_nonExplicitRules.ContainsKey(priority))
                    throw new ArgumentException(
                        $"Rule collision found in non-explicit rules (same from and to size = same priority of {priority}): " +
                        $"{_nonExplicitRules[priority]} and {rule}", nameof(rules));
                
                _nonExplicitRules.Add(priority, rule); // if we didn't check for collision beforehand, this would throw for duplicate key as well
                _potentiallyHandledTags = _potentiallyHandledTags.Union(rule.From);
            }
        }

        _numberOfRules = _explicitRules.Values.Sum(c => c.Count) + _nonExplicitRules.Count;
        if (_numberOfRules == 0) // check here instead of the top to avoid the multiple enumerations warning
            throw new ArgumentException("Must at least specify one rule.", nameof(rules));
        
        Rule? problematicRule = EnumerateAllRules().FirstOrDefault(r => !r.To.IsSubsetOf(_potentiallyHandledTags));
        if (problematicRule is not null)
            throw new ArgumentException(
                $"Rule {problematicRule} transforms into a set featuring the tags " +
                $"{problematicRule.To.Except(_potentiallyHandledTags)} which cannot be handled by any rule in this ruleset.",
                nameof(rules));
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
            Rule? match = FindBestMatchingRule(updatedTags);
            if (match is null)
                throw new InvalidOperationException(
                    $"This ruleset as it's configured cannot collapse {playerTags} fully; " +
                    $"no matching rule found for {updatedTags}.");

            updatedTags = match.Collapse(updatedTags);
            if (updatedTags.IsFullyCollapsed())
                return updatedTags;
        }

        throw new InvalidOperationException(
            $"Even after {maxIter} iterations (number of rules), the tags could be fully collapsed. " +
            $"Ended with {updatedTags}. Probably some sort of loop somewhere even though that shouldn't be possible.");
    }

    private Rule? FindBestMatchingRule(TagSet playerTags)
    {
        Rule? rule = FindMatchingExplicitRule(playerTags);
        if (rule is not null)
            return rule;

        return FindLongestMatchingNonExplicitRule(playerTags);
    }

    private Rule? FindMatchingExplicitRule(TagSet playerTags)
    {
        if (!_explicitRules.TryGetValue(playerTags.Count, out var explicitRuleForThatSize))
            return null;

        // we can be sure there's only one that matches thanks to the collision checks
        return explicitRuleForThatSize.SingleOrDefault(r => r.Matches(playerTags));
    }
    
    // first in a list sorted by priority descending
    private Rule? FindLongestMatchingNonExplicitRule(TagSet playerTags) =>
        _nonExplicitRules.Values.FirstOrDefault(r => r.Matches(playerTags));

    private IEnumerable<Rule> EnumerateAllRules() =>
        _explicitRules.SelectMany(kvp => kvp.Value).Concat(_nonExplicitRules.Values);
}