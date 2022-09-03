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

        if (to.Any(t => t is MasterTag) && to.Count != 1)
            throw new ArgumentException(
                "A rule can only collapse to one single master tag (or any number of ordinary tags).", nameof(to));
        
        From = from;
        To = to;
        Explicit = @explicit;
    }

    // TODO I'm very tired and going to sleep now but shouldn't this be subset and not superset?
    public bool Matches(TagSet playerTags) => Explicit ? From.Equals(playerTags) : From.IsSupersetOf(playerTags);

    public TagSet Collapse(TagSet playerTags)
    {
        if (!Matches(playerTags))
            throw new InvalidOperationException("Cannot collapse tags when rule doesn't match.");

        // for each player tag it checks if To has such a Tag template and if yes, it replaces it (retain meta-data).
        // everytime it does that, a new TagSet is created which is passed along for the next iteration and the final
        // one is returned from the Collapse function. If there's nothing to replace, no new instances are created.
        return playerTags.Aggregate(To, (current, playerTag) => current.ReplaceIfExists(playerTag));
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