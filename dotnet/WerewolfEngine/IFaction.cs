using WerewolfEngine.State;

namespace WerewolfEngine;

/// <summary>
/// A faction that can win in the game. Each role has one base faction. Each player has one
/// active faction.
/// Must be implemented immutably.
/// </summary>
public interface IFaction
{
    public string Name { get; }
    bool HasWon(GameState gameState);
}