using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

/// <summary>
/// A role that has certain actions. Each player has one or more roles.
/// Must be implemented immutably.
/// </summary>
public interface IRole
{
    /// <summary>
    /// Name of this role.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Default faction of this role. Players with this role belong to this faction unless they
    /// also have another role with a higher priority faction.
    /// </summary>
    public IFaction Faction { get; }
    
    /// <summary>
    /// Accessor to get to this specific role instance in any game state.
    /// </summary>
    public RoleAccessor Location { get; }

    /// <summary>
    /// Those roles that don't have any actions ever can be excluded from many things like the
    /// role order etc. Most roles have actions so it defaults to true but you can opt-out
    /// by setting it to false, for example for the villager role.
    /// </summary>
    public bool MightHaveAction => true;

    // something something get action (see L127 in the messy dotnet Program.cs for ideas)
    public IAction? GetNightAction(GameState game);
    public IAction? GetDayAction(GameState game) => null;  // most actions do not have a day action
}