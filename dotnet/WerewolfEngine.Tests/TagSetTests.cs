namespace WerewolfEngine.Tests;

public class TagSetTests
{
    [Fact]
    public void CanConstructEmptyTagSet()
    {
        // Arrange & Act
        TagSet tagSet = new();
        
        // Assert
        Assert.NotNull(tagSet);
    }

    [Fact]
    public void Equals_WithSameTags()
    {
        // Arrange
        Tag tag = new("A");
        
        TagSet a = new(tag);
        TagSet b = new(tag);
        
        // Assert
        Assert.Equal(a, b);
        Assert.StrictEqual(a, b);
    }
    
    [Fact]
    public void Equals_WithSameTagsButDifferentInstances()
    {
        // Arrange
        const string tag = "A";
        TagSet a = new(new Tag(tag));
        TagSet b = new(new Tag(tag));
        
        // Assert
        Assert.Equal(a, b);
        Assert.StrictEqual(a, b);
    }

    [Fact]
    public void AddOrReplace_AddsNewTag()
    {
        // Arrange
        Tag a = new("A");
        Tag b = new("B");

        TagSet set = new(a);
        
        // Act
        TagSet newSet = set.AddOrReplace(b);
        
        // Assert
        Assert.NotSame(set, newSet);
        Assert.Equal(1, set.Count);
        Assert.Equal(2, newSet.Count);
        Assert.Contains(b, newSet);
        Assert.Contains(a, newSet);
        Assert.True(newSet.Contains(b));
        Assert.True(newSet.Contains(a));
    }

    [Fact]
    public void AddOrReplace_CorrectlyUpdatesInstances()
    {
        // Arrange
        Tag a1 = new("A");
        Tag a2 = new("A");

        TagSet set = new(a1);
        
        // Act
        TagSet newSet = set.AddOrReplace(a2);
        
        // Assert
        Assert.Same(a2, newSet.Single());
        Assert.Same(a1, set.Single());
        Assert.NotSame(a1, newSet.Single());
    }

    [Fact]
    public void ReplaceIfExists_CorrectlyUpdatesInstances()
    {
        // Arrange
        Tag a1 = new("A");
        Tag a2 = new("A");

        TagSet set = new(a1);
        
        // Act
        TagSet newSet = set.ReplaceIfExists(a2);
        
        // Assert
        Assert.Same(a2, newSet.Single());
        Assert.Same(a1, set.Single());
        Assert.NotSame(a1, newSet.Single());
    }
    
    [Fact]
    public void ReplaceIfExists_DoesNothingIfNotExists()
    {
        // Arrange
        Tag a = new("A");
        Tag b = new("B");

        TagSet set = new(a);
        
        // Act
        TagSet newSet = set.ReplaceIfExists(b);
        
        // Assert
        Assert.Same(a, newSet.Single());
        Assert.Same(a, set.Single());
        Assert.NotSame(b, newSet.Single());
        Assert.Same(set, newSet); // actually only this or the other three necessary..
    }
}