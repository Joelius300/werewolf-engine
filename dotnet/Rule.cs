namespace WerewolfEngine;

public record Rule(TagSet From, TagSet To, bool Explicit)
{
    public bool Matches(TagSet playerTags) => Explicit ? From.Equals(playerTags) : From.IsSupersetOf(playerTags);

    public TagSet Collapse(TagSet playerTags)
    {
        if (!Matches(playerTags))
            throw new InvalidOperationException("Cannot collapse tags when rule doesn't match.");
        
        To.
    }
}