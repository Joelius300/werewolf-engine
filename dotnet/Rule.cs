namespace WerewolfEngine;

public record Rule(TagSet From, TagSet To, bool Explicit)
{
    public bool Matches(TagSet playerTags) => Explicit ? From.Equals(playerTags) : From.IsSupersetOf(playerTags);

    public TagSet Collapse(TagSet playerTags)
    {
        if (!Matches(playerTags))
            throw new InvalidOperationException("Cannot collapse tags when rule doesn't match.");

        var to = To.Clone();
        foreach (var playerTag in playerTags)
        {
            // replaces the template tag with the already present tag if there is one to copy the meta-data
            to = to.ReplaceIfExists(playerTag);
        }

        return to;
    }
}