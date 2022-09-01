using System.Collections.Immutable;

namespace WerewolfEngine;

// Use typed id for PlayerName as well as Tag id/name (avoid working with plain strings)
public record Player(string Name, PlayerState State, IReadOnlyList<IRole> Roles)
{
    public IImmutableSet<Tag> Tags { get; init; } = ImmutableHashSet<Tag>.Empty;

    public Player Kill()
    {
        if (State == PlayerState.Dead)
            throw new InvalidOperationException("Cannot kill a dead player.");

        return this with {State = PlayerState.Dead};
    }

    public Player Tag(Tag tag) => this with {Tags = Tags.Add(tag)};
}