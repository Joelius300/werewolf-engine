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
        Rule rule = new(new TagSet(a, b), new TagSet(b), true);
        
        // Act
        TagSet newTags = rule.Collapse(before);
        
        // Assert
        Assert.Equal(after, newTags);
    }
    
    private static IEnumerable<object[]> ExplicitRuleCollapseData()
    {
        Tag a = new("A");
        Tag b = new("B");

        // For rule: [{'A', 'B'} -> {'B'}]
        yield return new object[] {new TagSet(a, b), new TagSet(b)};
        yield return new object[] {new TagSet(b, a), new TagSet(b)};
    }
    
    [Theory]
    [MemberData(nameof(NonExplicitRuleCollapseData))]
    public void Collapse_NonExplicitRule(TagSet before, TagSet after)
    {
        // Arrange
        Tag a = new("A");
        Tag b = new("B");
        Rule rule = new(new TagSet(a, b), new TagSet(b), false);
        
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

        // For rule: ({'A', 'B'} -> {'B'})
        yield return new object[] {new TagSet(a, b), new TagSet(b)};
        yield return new object[] {new TagSet(b, a), new TagSet(b)};
        yield return new object[] {new TagSet(a, b, c), new TagSet(b, c)};
        yield return new object[] {new TagSet(a, b, c, d), new TagSet(b, c, d)};
    }

    [Theory]
    [MemberData(nameof(BothExplicitDataCollidesWith))]
    public void CollidesWith(Rule a, Rule b, bool collides, TagSet? offendingTagSet = null)
    {
        // test the implementation
        Assert.Equal(collides, a.CollidesWith(b));
        
        // test the test
        if (offendingTagSet is not null)
        {
            Assert.True(a.Matches(offendingTagSet));
            Assert.True(b.Matches(offendingTagSet));
        }
        else
        {
            // brute force that there are actually no combinations that match both
            var tagsUsedInTests = new TagSet(new Tag("A"), new Tag("B"));
            foreach (TagSet tagSet in tagsUsedInTests.GetCombinations())
            {
                Assert.False(a.Matches(tagSet) && b.Matches(tagSet));
            }
        }
    }

    private static IEnumerable<object[]> BothExplicitDataCollidesWith()
    {
        Tag a = new("A");
        Tag b = new("B");
        // only use a and b, this allows the test to brute force all possible combinations for matches and test the test
        
        // same from, both explicit -> collision (only possible way of collision when both are explicit)
        yield return new object[]
        {
            new Rule(new(a), new(), true),
            new Rule(new(a), new(), true),
            true,
            new TagSet(a)
        };
        
        // different from, both explicit -> no collision
        yield return new object[]
        {
            new Rule(new(a), new(), true),
            new Rule(new(b), new(), true),
            false
        };
        
        // a subset of b, both explicit -> no collision
        yield return new object[]
        {
            new Rule(new(a), new(), true),
            new Rule(new(a, b), new(), true),
            false
        };
        
        // b subset of a, both explicit -> no collision
        yield return new object[]
        {
            new Rule(new(a, b), new(), true),
            new Rule(new(a), new(), true),
            false
        };
        
        // same from, only a explicit -> collision
        yield return new object[]
        {
            new Rule(new(a), new(), true),
            new Rule(new(a), new(), false),
            true,
            new TagSet(a)
        };
        
        // a subset of b, only a explicit -> no collision
        yield return new object[]
        {
            new Rule(new(a), new(), true),
            new Rule(new(a, b), new(), false),
            false
        };
        
        // b subset of a, only a explicit -> collision
        yield return new object[]
        {
            new Rule(new(a, b), new(), true),
            new Rule(new(a), new(), false),
            true,
            new TagSet(a, b)
        };
        
        // same from, only b explicit -> collision
        yield return new object[]
        {
            new Rule(new(a), new(), false),
            new Rule(new(a), new(), true),
            true,
            new TagSet(a)
        };
        
        // a subset of b, only b explicit -> collision
        yield return new object[]
        {
            new Rule(new(a), new(), false),
            new Rule(new(a, b), new(), true),
            true,
            new TagSet(a, b)
        };
        
        // b subset of a, only b explicit -> no collision
        yield return new object[]
        {
            new Rule(new(a, b), new(), false),
            new Rule(new(a), new(), true),
            false
        };
        
        // both non-explicit -> could always collide
        yield return new object[]
        {
            new Rule(new(a), new(), false),
            new Rule(new(b), new(), false),
            true,
            new TagSet(a, b)
        };
    }
}