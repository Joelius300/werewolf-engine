using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

/// <summary>
/// Role for situations where an action is issued by the game itself.
/// </summary>
public class GodRole : IRole
{
    /// <summary>
    /// Singleton instance of the god role
    /// </summary>
    public static GodRole Instance { get; } = new();
    
    public string Name => "God-Role";
    public IAction? GetNightAction(GameState game) => null;
}