using WerewolfEngine.Actions;

namespace WerewolfEngine;

public interface IRole
{
    // something something get action (see L127 in the messy dotnet Program.cs for ideas)

    public IAction? GetNightAction(IGame game);
}