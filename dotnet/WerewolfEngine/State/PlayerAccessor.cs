namespace WerewolfEngine.State;

public record PlayerAccessor(string PlayerName)
{
    public Player GetFrom(GameState state) => state.Players[PlayerName];
    // also needed from PlayerCircle?
}