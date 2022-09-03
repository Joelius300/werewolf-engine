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
    [MemberData()]
    public void Matches_ExplicitRule()
    {
        Assert.True(false);
        // TODO with multiple TagSets check on one explicit rule. Then another test method for non-explicit (maybe reuse data member)
        // Use IsSubset and Equals. Also test those in TagSetTests :)
    }
}