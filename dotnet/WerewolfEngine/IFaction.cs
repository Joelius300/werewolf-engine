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
    
    /// <summary>
    /// Whether this faction is part of the werewolf factions that must be eliminated for the village to win.
    /// These are the same factions that are used to determine whether the werewolves have won.
    /// </summary>
    // There are multiple werewolf factions that need to be eliminated for the village to win, namely werewolves and
    // the white werewolf (at least).
    public bool IsWerewolfFaction { get; }
    
    /// <summary>
    /// Determines whether this faction has won according to the passed game state.
    /// </summary>
    bool HasWon(GameState gameState);
}