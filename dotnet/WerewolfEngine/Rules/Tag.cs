namespace WerewolfEngine.Rules;

/// <summary>
/// A single, immutable tag a player can be tagged with. Defined by the roles that distribute them.
/// Cannot define parameters and is for all intents and purposes a string (with meta-data).
/// Equality is only based on the id (string) of the tag.
/// </summary>
public class Tag : IEquatable<Tag>
{
    public string Identifier { get; }
    // public IReadyAction? CausingAction { get; init; }
    
    public Tag(string identifier)
    {
        Identifier = identifier;
    }

    public override string ToString() => $"'{Identifier}'";

    // maybe an EqualsPerfectly or something which also checks the meta-data (helpful, at least for tests)
    
    public bool Equals(Tag? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Identifier == other.Identifier;
    }

    public override bool Equals(object? obj) => obj is Tag tag && Equals(tag);
    public override int GetHashCode() => Identifier.GetHashCode();
    public static bool operator ==(Tag? left, Tag? right) => Equals(left, right);
    public static bool operator !=(Tag? left, Tag? right) => !Equals(left, right);
    public static explicit operator string(Tag tag) => tag.Identifier;
    public static implicit operator Tag(string identifier) => new(identifier); // very useful for defining rules and doesn't lose information
}