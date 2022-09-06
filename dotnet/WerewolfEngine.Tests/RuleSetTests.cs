using System.Collections;

namespace WerewolfEngine.Tests;

public class RuleSetTests
{
    [Fact]
    public void CannotConstructEmptyRuleset()
    {
        Assert.Throws<ArgumentException>("rules", () => new RuleSet(Array.Empty<Rule>()));
    }

    [Fact]
    public void CannotConstructRulesetWithConflictingExplicitRules()
    {
        // Arrange
        var rules = FromTuples(("ABC", "AB", true), ("CBA", "", true));
        
        // Act & Assert
        Assert.Throws<ArgumentException>("rules", () => new RuleSet(rules));
    }
    
    [Fact]
    public void CannotConstructRulesetWithConflictingNonExplicitRules()
    {
        // Arrange
        var rules = FromTuples(("ABC", "AB", false), ("DEF", "CE", false)); // same priority
        
        // Act & Assert
        Assert.Throws<ArgumentException>("rules", () => new RuleSet(rules));
    }

    [Fact]
    public void CannotConstructRulesetWithRuleThatTransformsIntoUnknownTags()
    {
        // Arrange
        var rules = FromTuples(("ABC", "AB", true), ("CBA", "D", true));
        
        // Act & Assert
        Assert.Throws<ArgumentException>("rules", () => new RuleSet(rules));
    }

    [Fact]
    public void Collapse_MatchesExplicitBeforeNonExplicit()
    {
        // Arrange
        var rules = FromTuples(("ABC", "", true), ("AB", "A", false)).ToArray();
        RuleSet ruleSet = new(rules);
        var playerTags = CharsAsTags("ABC");

        // Act
        var collapsed = ruleSet.Collapse(playerTags);
        
        // Assert
        // used explicit rule instead of non-explicit even though both match
        Assert.True(rules.All(r => r.Matches(playerTags)));
        Assert.Equal(new TagSet(), collapsed);
    }
    
    [Fact]
    public void Collapse_MatchesMultipleOfEachTypeBeforeBeingFullyCollapsed()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(
                ("ABCD", "ABC", true), // ABCD -> ABC
                ("AC", "AD", false), // ABC -> ABD
                ("AB", "A", false), // ABD -> AD
                ("AD", "", true) // AD -> ''
            )
        );
        var playerTags = CharsAsTags("ABCD");

        // Act
        var collapsed = ruleSet.Collapse(playerTags);
        
        // Assert
        Assert.Equal(new TagSet(), collapsed);
    }
    
    [Fact]
    public void Collapse_ReturnsImmediatelyIfAlreadyFullyCollapsed()
    {
        // Arrange
        RuleSet ruleSet = ValidExampleRuleSet();
        var playerTags = new TagSet(); // empty is already fully collapsed

        // Act
        var collapsed = ruleSet.Collapse(playerTags);
        
        // Assert
        Assert.Equal(playerTags, collapsed);
        Assert.Same(playerTags, collapsed); // might be testing for implementation details here
    }
    
    [Fact]
    public void Collapse_ThrowsIfTagDoesntExistInAnyRule()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(("ABC", "", true)));
        var playerTags = CharsAsTags("ABCD");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ruleSet.Collapse(playerTags));
    }
    
    [Fact]
    public void Collapse_ThrowsWhenNoMatchingRuleCanBeFoundImmediately()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(("ABC", "", true)));
        var playerTags = CharsAsTags("AB");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ruleSet.Collapse(playerTags));
    }
    
    [Fact]
    public void Collapse_ThrowsWhenNoMatchingRuleCanBeFoundAfterMatchingExplicitRule()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(("ABC", "AB", true)));
        var playerTags = CharsAsTags("ABC");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ruleSet.Collapse(playerTags));
    }
    
    [Fact]
    public void Collapse_ThrowsWhenNoMatchingRuleCanBeFoundAfterMatchingNonExplicitRule()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(("AB", "B", false)));
        var playerTags = CharsAsTags("ABC");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ruleSet.Collapse(playerTags));
    }

    private static TagSet CharsAsTags(string tags) => new(tags.Select(c => new Tag(c.ToString())));

    private static Rule FromStrings(string from, string to, bool @explicit) =>
        new(CharsAsTags(from), CharsAsTags(to), @explicit);

    private static IEnumerable<Rule> FromTuples(params (string from, string to, bool @explicit)[] args) =>
        args.Select(t => FromStrings(t.from, t.to, t.@explicit));

    // for when you need a valid ruleset with different kinds of rules but it doesn't matter what rules there are
    private static RuleSet ValidExampleRuleSet() => new(FromTuples(
            ("ABCD", "ABC", true),
            ("AC", "AD", false),
            ("AB", "A", false),
            ("AD", "", true)
        )
    );
}