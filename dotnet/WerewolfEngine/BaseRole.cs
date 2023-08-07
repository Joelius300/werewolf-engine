using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

public abstract class BaseRole<TDerived> : IRole
    where TDerived : class, IRole
{
    public string Name { get; }
    protected RoleAccessor<TDerived> Location { get; }
    
    protected BaseRole(string roleName, string playerName)
    {
        Name = roleName;
        Location = new RoleAccessor<TDerived>(new PlayerAccessor(playerName), roleName);
    }

    public abstract IAction? GetNightAction(GameState game);
}