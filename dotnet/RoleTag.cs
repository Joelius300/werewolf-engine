namespace WerewolfEngine;

public class RoleTag : Tag
{
    public IRole Role { get; }

    public RoleTag(string identifier, IReadyAction? causingAction, IRole role) : base(identifier, causingAction)
    {
        Role = role;
    }
}