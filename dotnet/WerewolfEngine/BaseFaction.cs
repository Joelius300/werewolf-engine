using WerewolfEngine.State;

namespace WerewolfEngine;

/// <summary>
/// Base class for factions that already implements equality based purely on the name of the faction.
/// </summary>
public abstract class BaseFaction : IFaction, IEquatable<BaseFaction>
{
    public string Name { get; }
    public bool IsWerewolfFaction { get; }

    protected BaseFaction(string name, bool isWerewolfFaction)
    {
        Name = name;
        IsWerewolfFaction = isWerewolfFaction;
    }

    public abstract bool HasWon(GameState gameState);

    public bool Equals(BaseFaction? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((BaseFaction) obj);
    }

    public override int GetHashCode() => Name.GetHashCode();

    public static bool operator ==(BaseFaction? left, BaseFaction? right) => Equals(left, right);

    public static bool operator !=(BaseFaction? left, BaseFaction? right) => !Equals(left, right);
}