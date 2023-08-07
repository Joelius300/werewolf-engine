namespace WerewolfEngine.State;

public static class GameStateExtensions
{
    public static IEnumerable<IFaction> GetFactionsInPlay(this GameState gameState) =>
        gameState.Players.Select(p => p.ActiveFaction).Distinct();
    
    public static IEnumerable<IRole> GetRolesInPlay(this GameState gameState) =>
        gameState.Players.SelectMany(p => p.Roles);
}