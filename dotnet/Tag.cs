namespace WerewolfEngine;

public class Tag : IEquatable<Tag>
{
    public string Identifier { get; }
    public IReadyAction? CausingAction { get; }

    public Tag(string identifier, IReadyAction? causingAction)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        CausingAction = causingAction;
    }

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

    public override string ToString() => $"[{Identifier}, {CausingAction}]";
}