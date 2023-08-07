using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

/// <summary>
/// A role that has certain actions. Each player has one or more roles.
/// Must be implemented immutably.
/// </summary>
public interface IRole
{
    public string Name { get; }
    
    // something something get action (see L127 in the messy dotnet Program.cs for ideas)
    public IAction? GetNightAction(GameState game);
    public IAction? GetDayAction(GameState game) => null;  // most actions do not have a day action
}