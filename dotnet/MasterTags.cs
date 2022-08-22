namespace WerewolfEngine;

public static class MasterTags
{
    public static Tag Killed(IReadyAction? causingAction) => new("killed", causingAction);
    public static RoleTag RoleAdded(IRole role, IReadyAction? causingAction) => new("role_added", causingAction, role);

    public static RoleTag RoleRemoved(IRole role, IReadyAction? causingAction) =>
        new("role_removed", causingAction, role);
}