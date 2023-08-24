using WerewolfEngine.Rules;

namespace WerewolfEngine.Tests.Rules;

public class RuleSetTests
{
    [Fact]
    public void CannotConstructEmptyRuleset()
    {
        Assert.Throws<ArgumentException>("rules", () => new RuleSet(Array.Empty<Rule>(), Array.Empty<string>()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CannotConstructRulesetWithRulesWithIdenticalFromAndType(bool @explicit)
    {
        // Arrange
        var rules = FromTuples(("ABC", "AB", @explicit), ("ABC", "AC", @explicit));

        // Act & Assert
        Assert.Throws<ArgumentException>("rules", () => new RuleSet(rules, Array.Empty<string>()));
    }

    [Fact]
    public void CannotConstructRulesetWithConflictingExplicitRules()
    {
        // Arrange
        var rules = FromTuples(("ABC", "AB", true), ("ABC", "", true));

        // Act & Assert
        Assert.Throws<ArgumentException>("rules", () => new RuleSet(rules, Array.Empty<string>()));
    }

    [Fact]
    public void CanConstructRulesetWithNonExplicitRulesOfDifferentPriorities()
    {
        // Arrange
        var rules = FromTuples(("AB", "B", false), ("AC", "AB", false));

        // Act
        var ruleSet = new RuleSet(rules, Array.Empty<string>());

        // Assert
        Assert.NotNull(ruleSet);
    }

    [Fact]
    public void CanConstructRulesetWithNonExplicitRulesOfSamePriorities()
    {
        // Arrange
        var rules = FromTuples(("AB", "B", false), ("AC", "C", false));

        // Act
        var ruleSet = new RuleSet(rules, Array.Empty<string>());

        // Assert
        Assert.NotNull(ruleSet);
    }

    [Fact]
    public void CannotConstructRulesetWithRuleThatTransformsIntoUnknownTags()
    {
        // Arrange
        var rules = FromTuples(("ABC", "AB", true), ("CBA", "D", true));

        // Act & Assert
        Assert.Throws<ArgumentException>("rules", () => new RuleSet(rules, Array.Empty<string>()));
    }

    [Fact]
    public void Collapse_MatchesExplicitBeforeNonExplicit()
    {
        // Arrange
        var rules = FromTuples(("ABC", "", true), ("AB", "A", false)).ToArray();
        RuleSet ruleSet = new(rules, Array.Empty<string>());
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
            ), Array.Empty<string>()
        );
        var playerTags = CharsAsTags("ABCD");

        // Act
        var collapsed = ruleSet.Collapse(playerTags);

        // Assert
        Assert.Equal(new TagSet(), collapsed);
    }

    [Fact]
    public void Collapse_MatchesNonExplicitRuleWithHighestPriority()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(
                ("ABCD", "", false), // highest reduction/priority, so it should match this one
                ("ABC", "", false),
                ("AB", "", false),
                ("A", "", false),
                ("E", "", true)
            ), Array.Empty<string>()
        );
        // the only way to collapse this is ABCDE -> E -> '', all the other rules would lead nowhere so if it doesn't
        // throw it was successful and chose the correct rule to apply.
        var playerTags = CharsAsTags("ABCDE");

        // Act
        var collapsed = ruleSet.Collapse(playerTags);

        // Assert
        Assert.Equal(new TagSet(), collapsed);
    }

    [Fact] // TODO for all collision tests, add one that checks if the meta-data has been merged
    public void Collapse_CanHandleRuleCollisionWhenBothRulesResultInTheSameNextStep()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(
                // first two have the same priority and just remove A (when AB or AC is contained in the set)
                ("AB", "B", false),
                ("AC", "C", false),
                ("BC", "", true) // fully collapse BC
            ), Array.Empty<string>()
        );
        var playerTags = CharsAsTags("ABC");
        /* This can collapse either through
         * ABC -> BC -> ''
         * or
         * ABC -> BC -> ''
         *
         * In both cases, the intermediate step is BC so the ruleset can recover from the collision.
         */

        // Act
        var collapsed = ruleSet.Collapse(playerTags);

        // Assert
        Assert.Equal(new TagSet(), collapsed);
    }

    [Fact] // TODO for all collision tests, add one that checks if the meta-data has been merged
    public void Collapse_CanHandleRuleCollisionWhenBothRulesResultInTheSameFullyCollapsedSet()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(
                ("ABC", "AC", false), // ABCD -> ACD
                ("ACD", "AD", true),
                ("AD", "", true),
                ("ACD", "CD", false), // ABCD -> BCD
                ("BCD", "", true)
            ), Array.Empty<string>()
        );
        var playerTags = CharsAsTags("ABCD");
        /* This can collapse either through
         * ABCD -> ACD -> AD -> ''
         * or
         * ABCD -> BCD -> ''
         *
         * In both cases, the fully collapsed set is '' so the ruleset can recover from the collision.
         */

        // Act
        var collapsed = ruleSet.Collapse(playerTags);

        // Assert
        Assert.Equal(new TagSet(), collapsed);
    }

    [Fact]
    public void Collapse_ThrowsOnRuleCollisionWhenFullyCollapsedSetIsDifferent()
    {
        // Arrange
        var rules = FromTuples(
                ("ABC", "AC", false), // ABCD -> ACD
                ("ACD", "AD", true),
                ("ACD", "CD", false), // ABCD -> BCD
                ("BCD", "", true)
            )
            .ToList();
        rules.Add(new Rule(CharsAsTags("AD"), new(MasterTag.Killed), true));
        var ruleSet = new RuleSet(rules, Array.Empty<string>());
        var playerTags = CharsAsTags("ABCD");
        /* This can collapse either through
         * ABCD -> ACD -> AD -> killed
         * or
         * ABCD -> BCD -> ''
         *
         * Since they end in different tag sets, it cannot recover and throws and exception.
         */

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ruleSet.Collapse(playerTags));
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
        RuleSet ruleSet = new(FromTuples(("ABC", "", true)), Array.Empty<string>());
        var playerTags = CharsAsTags("ABCD");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ruleSet.Collapse(playerTags));
    }

    [Fact]
    public void Collapse_ThrowsWhenNoMatchingRuleCanBeFoundImmediately()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(("ABC", "", true)), Array.Empty<string>());
        var playerTags = CharsAsTags("AB");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ruleSet.Collapse(playerTags));
    }

    [Fact]
    public void Collapse_ThrowsWhenNoMatchingRuleCanBeFoundAfterMatchingExplicitRule()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(("ABC", "AB", true)), Array.Empty<string>());
        var playerTags = CharsAsTags("ABC");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ruleSet.Collapse(playerTags));
    }

    [Fact]
    public void Collapse_ThrowsWhenNoMatchingRuleCanBeFoundAfterMatchingNonExplicitRule()
    {
        // Arrange
        RuleSet ruleSet = new(FromTuples(("AB", "B", false)), Array.Empty<string>());
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
        ), Array.Empty<string>()
    );
}