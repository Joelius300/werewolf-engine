namespace WerewolfEngine.Rules;

/// <summary>
/// An immutable definition of a transformation (reduction) from one set of tags to another.
/// </summary>
// Maybe will need to be non-sealed in the future for implementing more special Rules with maps for meta-data, e.g.
// killed -> killed_by_werewolf, killed_by_witch would mean that To has to contain killed, From the other to and during
// collapse, the meta-data of killed_by_werewolf and killed_by_witch would be merged into killed. This would be a more
// explicit system as the CombineMetaData flag but for the same purpose more or less. However, this system here would
// allow the building of trees and connecting of converging branches by aggregating the rules together which also needs
// to deal with the meta-data in a sensible matter. That way, you could reduce many rules of multiple steps (with
// potentially many non-explicit rules) into a single explicit rule that knows exactly how to deal with the meta-data.
public sealed class Rule : IEquatable<Rule>
{
    public TagSet From { get; }
    public TagSet To { get; }
    public bool Explicit { get; }
    
    public int FromSize => From.Count;
    public int ToSize => To.Count;
    
    public Rule(TagSet from, TagSet to, bool @explicit)
    {
        if (from.Count == 0)
            throw new ArgumentException("Cannot create rule for an empty From TagSet.", nameof(from));

        if (to.Count > from.Count)
            throw new ArgumentException("Cannot collapse tags to a larger tag set.", nameof(to));

        if (to == from)
            throw new ArgumentException("Cannot collapse tag set to itself - change must be guaranteed.", nameof(to));

        /*
        // I may need to revisit this because right now I can imagine situations where you are killed _and_ get a new role
        // (for certain game modes this might be relevant).
        if (to.Any(t => t is MasterTag) && to.Count != 1)
            throw new ArgumentException(
                "A rule can only collapse to one single master tag (or any number of ordinary tags).", nameof(to));
        */
        
        From = from;
        To = to;
        Explicit = @explicit;
    }

    /// Returns true, if a tag set can exist, that matches both rules
    public bool CollidesWith(Rule otherRule)
    {
        if (From == otherRule.From)
            return true;

        return (Explicit, otherRule.Explicit) switch
        {
            (true, true) => false, // both explicit but different from, won't ever collide
            (false, false) => true, // both non-explicit, could always collide
            // could collide if the non-explicit rule's from is a subset of the explicit one's
            (false, true) => From.IsSubsetOf(otherRule.From),
            (true, false) => From.IsSupersetOf(otherRule.From),
        };
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