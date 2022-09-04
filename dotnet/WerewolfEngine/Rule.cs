namespace WerewolfEngine;

public class Rule : IEquatable<Rule>
{
    public TagSet From { get; }
    public TagSet To { get; }
    public bool Explicit { get; }
    
    public Rule(TagSet from, TagSet to, bool @explicit)
    {
        if (from.Count == 0)
            throw new ArgumentException("Cannot create rule for an empty From TagSet.", nameof(from));

        // I may need to revisit this because right now I can imagine situations where you are killed _and_ get a new role
        // (for certain game modes this might be relevant).
        if (to.Any(t => t is MasterTag) && to.Count != 1)
            throw new ArgumentException(
                "A rule can only collapse to one single master tag (or any number of ordinary tags).", nameof(to));
        
        From = from;
        To = to;
        Explicit = @explicit;
    }

    public bool Matches(TagSet playerTags) => Explicit ? From.Equals(playerTags) : From.IsSubsetOf(playerTags);

    public TagSet Collapse(TagSet playerTags)
    {
        // we could also say that collapsing tags with a rule that doesn't match just returns them unaltered, we'll see
        if (!Matches(playerTags))
            throw new InvalidOperationException("Cannot collapse tags when rule doesn't match.");

        // for each player tag it checks if To has such a Tag template and if yes, it replaces it with the one present
        // in the player tags (to retain meta-data which is present in the player's tag but not in the template).
        // If there's nothing to replace, no new instances are created.
        // Maybe we're working with an anti-pattern here because we say tag a1 and tag a2 (both "A") are equal even though
        // they may have different Meta-Data and although we say they're equal, we actually care about which instance we get.
        // This also prevents us from using Intersect among other things because it takes all the instances that are present
        // in both sets without caring or defining where those instance come from since they are equal (but for us they're not).
        TagSet adjustedTo = playerTags.Aggregate(To, (current, playerTag) => current.ReplaceIfExists(playerTag));
        
        if (Explicit)
        {
            // replace all the player's tags with the new ones.
            return adjustedTo;
        }

        // remove the From tags and replace them with the To tags
        return playerTags.Except(From).Union(adjustedTo);
    }

    public override string ToString() => $"{(Explicit ? '[' : '(')}{From} -> {To}{(Explicit ? ']' : ')')}";

    public bool Equals(Rule? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return From.Equals(other.From) && To.Equals(other.To) && Explicit == other.Explicit;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Rule) obj);
    }

    public override int GetHashCode() => HashCode.Combine(From, To, Explicit);
    public static bool operator ==(Rule? left, Rule? right) => Equals(left, right);
    public static bool operator !=(Rule? left, Rule? right) => !Equals(left, right);
}