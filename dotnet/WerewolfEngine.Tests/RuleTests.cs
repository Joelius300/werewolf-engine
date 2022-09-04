namespace WerewolfEngine.Tests;

public class RuleTests
{
    [Fact]
    public void CannotConstructWithEmptyFrom()
    {
        // Arrange
        TagSet emptySet = new();
        
        // Act & Assert
        Assert.Throws<ArgumentException>("from", () => new Rule(emptySet, emptySet, true));
        Assert.Throws<ArgumentException>("from", () => new Rule(emptySet, emptySet, false));
    }

    [Fact(Skip = "There's currently only one master tag implemented.")]
    public void CannotConstructWithMoreThanOneMasterTag()
    {
        Assert.True(false); // no Assert.Fail?
        // TODO Once there are more than one master tag
    }

    [Theory]
    [MemberData(nameof(ExplicitRuleMatchData))]
    public void Matches_ExplicitRule(TagSet tagSet, bool matches)
    {
        // Arrange
        Tag a = new("A");
        Tag b = new("B");
        Rule rule = new(new TagSet(a, b), new TagSet(), true);
        
        // Act & Assert
        Assert.Equal(matches, rule.Matches(tagSet));
    }

    private static IEnumerable<object[]> ExplicitRuleMatchData()
    {
        Tag a = new("A");
        Tag b = new("B");

        yield return new object[] {new TagSet(a, b), true};
        yield return new object[] {new TagSet(b, a), true};
        yield return new object[] {new TagSet(a), false};
        yield return new object[] {new TagSet(b), false};
        yield return new object[] {new TagSet(), false};
    }
    
    [Theory]
    [MemberData(nameof(NonExplicitRuleMatchData))]
    public void Matches_NonExplicitRule(TagSet tagSet, bool matches)
    {
        // Arrange
        Tag a = new("A");
        Tag b = new("B");
        Rule rule = new(new TagSet(a, b), new TagSet(), false);
        
        // Act & Assert
        Assert.Equal(matches, rule.Matches(tagSet));
    }

    private static IEnumerable<object[]> NonExplicitRuleMatchData()
    {
        Tag a = new("A");
        Tag b = new("B");
        Tag c = new("C");
        Tag d = new("D");

        yield return new object[] {new TagSet(a, b), true};
        yield return new object[] {new TagSet(a, b, c), true};
        yield return new object[] {new TagSet(a, b, c, d), true};
        yield return new object[] {new TagSet(a, c, d), false};
        yield return new object[] {new TagSet(a), false};
        yield return new object[] {new TagSet(b), false};
        yield return new object[] {new TagSet(), false};
    }

    [Theory]
    [MemberData(nameof(ExplicitRuleCollapseData))]
    public void Collapse_ExplicitRule(TagSet before, TagSet after)
    {
        // Arrange
        Tag a = new("A");
        Tag b = new("B");
        Rule rule = new(new TagSet(a, b), new TagSet(), true);
        
        // Act
        TagSet newTags = rule.Collapse(before);
        
        // Assert
        Assert.Equal(after, newTags);
    }
    
    private static IEnumerable<object[]> ExplicitRuleCollapseData()
    {
        Tag a = new("A");
        Tag b = new("B");

        // For rule: [{'A', 'B'} -> {}]
        yield return new object[] {new TagSet(a, b), new TagSet()};
        yield return new object[] {new TagSet(b, a), new TagSet()};
    }
    
    [Theory]
    [MemberData(nameof(NonExplicitRuleCollapseData))]
    public void Collapse_NonExplicitRule(TagSet before, TagSet after)
    {
        // Arrange
        Tag a = new("A");
        Tag b = new("B");
        Rule rule = new(new TagSet(a, b), new TagSet(), false);
        
        // Act
        TagSet newTags = rule.Collapse(before);
        
        // Assert
        Assert.Equal(after, newTags);
    }
    
    private static IEnumerable<object[]> NonExplicitRuleCollapseData()
    {
        Tag a = new("A");
        Tag b = new("B");
        Tag c = new("C");
        Tag d = new("D");

        // For rule: ({'A', 'B'} -> {})
        yield return new object[] {new TagSet(a, b), new TagSet()};
        yield return new object[] {new TagSet(b, a), new TagSet()};
        yield return new object[] {new TagSet(a, b, c), new TagSet(c)};
        yield return new object[] {new TagSet(a, b, c, d), new TagSet(c, d)};
    }
}