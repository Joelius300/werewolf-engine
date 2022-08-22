namespace WerewolfEngine;

public class RoleTag : Tag
{
    public override IRole Parameter { get; }

    public RoleTag(string identifier, IReadyAction? causingAction, IRole role) : base(identifier, causingAction, role)
    {
        // this gives warnings although we can be sure that it's assigned through the base class
    }
}