namespace WerewolfEngine;

public record Player(string Name, PlayerState State, IReadOnlyList<IRole> Roles)
{
    public Player Kill()
    {
        if (State == PlayerState.Dead)
            throw new InvalidOperationException("Cannot kill a dead player.");

        return this with {State = PlayerState.Dead};
    }
}