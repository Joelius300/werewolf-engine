using WerewolfEngine.Rules;

namespace WerewolfEngine.State;

public static class PlayerExtensions
{
    public static Player Tag(this Player player, Tag tag) => player with {Tags = player.Tags.Add(tag)};
    
    public static Player Kill(this Player player) => player with {State = PlayerState.Dead};

    public static TRole GetRole<TRole>(this Player player, string roleName)
        where TRole : class, IRole
    {
        var role = player.Roles.SingleOrDefault(r => r.Name == roleName);
        if (role is null)
            throw new ArgumentException($"Player '{player.Name}' doesn't have role '{roleName}'.");

        if (role is not TRole castRole)
            throw new ArgumentException($"Role '{roleName}' found on Player '{player.Name}' but isn't of type" +
                                        $"{typeof(TRole).FullName}!");

        return castRole;
    }
    
    public static Player UpdateRole<TRole>(this Player player, string roleName, Func<TRole, TRole> updater)
        where TRole : class, IRole
    {
        var role = player.GetRole<TRole>(roleName);
        return player with {Roles = player.Roles.Replace(role, updater(role), GodRole.NameOnlyComparer)};
    }
}