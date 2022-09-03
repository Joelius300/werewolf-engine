namespace WerewolfEngine;

// Although they are handy sometimes, dump record here because
// we don't want to be able to construct rules with an empty from TagSet
// and also if the To TagSet contains a master tag, we need to be sure it's the only tag
// (cannot collapse to more than one master tag).
public record Rule(TagSet From, TagSet To, bool Explicit)
{
    public bool Matches(TagSet playerTags) => Explicit ? From.Equals(playerTags) : From.IsSupersetOf(playerTags);

    public TagSet Collapse(TagSet playerTags)
    {
        if (!Matches(playerTags))
            throw new InvalidOperationException("Cannot collapse tags when rule doesn't match.");

        // for each player tag it checks if To has such a Tag template and if yes, it replaces it (retain meta-data).
        return playerTags.Aggregate(To, (current, playerTag) => current.ReplaceIfExists(playerTag));
    }
}