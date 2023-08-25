using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

/// <summary>
/// Role for situations where an action is issued by the game itself.
/// </summary>
public class GodRole : IRole
{
    private const string RoleName = "God-Role";
    public const string KilledByVillage = "killed_by_village";

    /// <summary>
    /// Singleton instance of the god role
    /// </summary>
    public static GodRole Instance { get; } = new();

    public static RoleAccessor<GodRole> Accessor { get; } = new GodRoleAccessor();

    public string Name => RoleName;
    
    public IFaction Faction => throw new NotImplementedException("God doesn't have a faction.");
    RoleAccessor IRole.Location => Accessor;
    
    public IAction? GetNightAction(GameState game) => null;

    private record GodRoleAccessor() : RoleAccessor<GodRole>(
        new PlayerAccessor("God, like straight from the internals of the game, I doubt you'll find 'em"),
        GodRole.RoleName)
    {
        public override GodRole GetFrom(GameState state) => Instance;
    }
    
    private sealed class NameEqualityComparer : IEqualityComparer<IRole>
    {
        public bool Equals(IRole? x, IRole? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Name == y.Name;
        }

        public int GetHashCode(IRole obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    /// <summary>
    /// EqualityComparer for IRole that only respects the name of the role (which in our case should be just as
    /// unique as the type of role, e.g. "witch" ↔ WitchRole. Since a player can only have one instance of a role,
    /// this allows fetching a certain role on a player.
    /// </summary>
    public static IEqualityComparer<IRole> NameOnlyComparer { get; } = new NameEqualityComparer();
}