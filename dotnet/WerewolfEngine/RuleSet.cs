using System.Collections;

namespace WerewolfEngine;

/// <summary>
/// A set of rules defining how any tag set can be iteratively reduced to a set of master tags the
/// core engine can handle.
/// </summary>
public class RuleSet
{
    private readonly IDictionary<int, ICollection<Rule>> _explicitRules;
    private readonly SortedList<int, Rule> _nonExplicitRules;

    public RuleSet(IEnumerable<Rule> rules)
    {
        _explicitRules = new Dictionary<int, ICollection<Rule>>();
        _nonExplicitRules = new SortedList<int, Rule>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
        foreach (var rule in rules)
        {
            if (rule.Explicit)
            {
                if (!_explicitRules.TryGetValue(rule.FromSize, out var explicitRules))
                {
                    explicitRules = new List<Rule>();
                    _explicitRules.Add(rule.FromSize, explicitRules);
                }
                explicitRules.Add(rule);
            }
            else
            {
                _nonExplicitRules.Add(rule.FromSize, rule);
            }
        }
    }

    protected Rule? FindFirstMatchingRule(TagSet playerTags)
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

        return explicitRuleForThatSize.SingleOrDefault(r => r.Matches(playerTags));
    }

    private Rule? FindLongestMatchingNonExplicitRule(TagSet playerTags) =>
        _nonExplicitRules.Values.FirstOrDefault(r => r.Matches(playerTags));
}