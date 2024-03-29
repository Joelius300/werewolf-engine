using WerewolfEngine.Rules;

namespace WerewolfEngine.State;

public static class GameStateExtensions
{
    public static IEnumerable<IFaction> GetFactionsInPlay(this GameState gameState) =>
        gameState.Players.Select(p => p.ActiveFaction).Distinct();

    public static IEnumerable<IRole> GetRolesInPlay(this GameState gameState) =>
        gameState.Players.SelectMany(p => p.Roles);

    public static GameState UpdatePlayer(this GameState gameState, string playerName, Func<Player, Player> updater) =>
        gameState with
        {
            Players = gameState.Players.UpdatePlayer(playerName, updater)
        };

    public static GameState UpdatePlayer(this GameState gameState, PlayerAccessor accessor,
        Func<Player, Player> updater) => gameState.UpdatePlayer(accessor.PlayerName, updater);

    public static GameState TagPlayer(this GameState gameState, string playerName, Tag tag) =>
        gameState.UpdatePlayer(playerName, p => p.Tag(tag));

    public static bool IsAlive(this GameState gameState, string playerName) =>
        gameState.Players[playerName].State == PlayerState.Alive;

    // this check is almost always necessary, some smarter way to do it would be great
    public static void CheckAlive(this GameState gameState, string playerName)
    {
        if (!gameState.IsAlive(playerName))
            throw new ArgumentException($"Player '{playerName}' is not alive!");
    }

    public static bool HasTag(this GameState gameState, string playerName, Tag tag) =>
        gameState.Players[playerName].Tags.Contains(tag);

    public static GameState KillPlayer(this GameState gameState, string playerName) =>
        gameState.UpdatePlayer(playerName, p => p.Kill());

    public static GameState UpdateRole<TRole>(this GameState gameState, RoleAccessor<TRole> accessor,
        Func<TRole, TRole> updater)
        where TRole : class, IRole
    {
        return gameState.UpdatePlayer(accessor.Player, p => p.UpdateRole(accessor.RoleName, updater));
    }
}