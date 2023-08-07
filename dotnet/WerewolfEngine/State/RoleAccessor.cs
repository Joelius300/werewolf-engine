using System.Diagnostics;

namespace WerewolfEngine.State;

public record RoleAccessor(PlayerAccessor Player, string RoleName)
{
    public virtual IRole GetFrom(GameState state) =>
        Player.GetFrom(state).Roles.SingleOrDefault(r => r.Name == RoleName) ??
        throw new ArgumentException(
            $"No (or multiple) instance of role '{RoleName}' found on player '{Player.PlayerName}'");
    
    // also needed from PlayerCircle?
}

public record RoleAccessor<TRole>(PlayerAccessor Player, string RoleName) : RoleAccessor(Player, RoleName)
    where TRole : class, IRole
{
    public override TRole GetFrom(GameState state)
    {
        var roleRaw = base.GetFrom(state);
        if (roleRaw is TRole role) return role;

        // the non generic role accessor should never return null
        Debug.Assert(roleRaw is not null, "role is not null");
        
        throw new InvalidCastException($"Role '{RoleName}' found on player '{Player.PlayerName}' but" +
                                       $"it was not of type {typeof(TRole).FullName}!");
    }
}