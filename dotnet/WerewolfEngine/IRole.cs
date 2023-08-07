using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

public interface IRole
{
    public string Name { get; }
    
    // something something get action (see L127 in the messy dotnet Program.cs for ideas)
    public IAction? GetNightAction(GameState game);
    public IAction? GetDayAction(GameState game);
}