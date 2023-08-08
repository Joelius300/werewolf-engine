using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

public abstract record BaseRole<TDerived> : IRole
    where TDerived : class, IRole
{
    public string Name { get; }
    public IFaction Faction { get; }
    public virtual bool MightHaveAction => true;

    RoleAccessor IRole.Location => Location;
    public RoleAccessor<TDerived> Location { get; }
    
    protected BaseRole(string roleName, IFaction faction, string playerName)
    {
        Name = roleName;
        Faction = faction;
        Location = new RoleAccessor<TDerived>(new PlayerAccessor(playerName), roleName);
    }

    public abstract IAction? GetNightAction(GameState game);
}