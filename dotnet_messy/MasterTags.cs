namespace WerewolfEngine;

// In Rust this would be an enum with parameters. All the other tags should at least for now not contain any parameters
// and be as simple as a string with the additional meta-information about causing player, role and action.
public static class MasterTags
{
    public static Tag Killed(IReadyAction? causingAction) => new("killed", causingAction);
    public static RoleTag RoleAdded(IRole role, IReadyAction? causingAction) => new("role_added", causingAction, role);

    public static RoleTag RoleRemoved(IRole role, IReadyAction? causingAction) =>
        new("role_removed", causingAction, role);
}