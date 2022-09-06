using System.Collections;

namespace WerewolfEngine;

/// <summary>
/// A set of rules defining how any tag set can be iteratively reduced to a set of master tags the
/// core engine can handle.
/// </summary>
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