using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

/// <summary>
/// Role for situations where an action is issued by the game itself.
/// </summary>
public class GodRole : IRole
{
    private const string RoleName = "God-Role";

    /// <summary>
    /// Singleton instance of the god role
    /// </summary>
    public static GodRole Instance { get; } = new();

    public static RoleAccessor<GodRole> Accessor { get; } = new GodRoleAccessor();

    public string Name => RoleName;
    public IAction? GetNightAction(GameState game) => null;

    private record GodRoleAccessor() : RoleAccessor<GodRole>(
        new PlayerAccessor("God, like straight from the internals of the game, I doubt you'll find 'em"),
        GodRole.RoleName)
    {
        public override GodRole GetFrom(GameState state) => Instance;
    }
}