using System.Collections.Immutable;
using System.Text.Json.Serialization;
using WerewolfEngine.Actions;

namespace WerewolfEngine.State;

public record GameState(
    GamePhase Phase,
    int Round,
    GameActionState State,
    IFaction? Winner,
    PlayerCircle Players,
    IAction? CurrentAction,
    IImmutableList<IAction> NextActions)
{
    /* Although sexy, they don't work for the generic role accessor which is a bummer so let's not confuse ourselves.
    public Player this[PlayerAccessor accessor] => accessor.GetFrom(this);
    public IRole this[RoleAccessor accessor] => accessor.GetFrom(this);
    */

    public Player Query(PlayerAccessor accessor) => accessor.GetFrom(this);
    public IRole Query(RoleAccessor accessor) => accessor.GetFrom(this);
    public TRole Query<TRole>(RoleAccessor<TRole> accessor) where TRole : class, IRole => accessor.GetFrom(this);
}