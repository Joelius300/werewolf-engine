using System.Collections.Immutable;
using WerewolfEngine.Rules;

namespace WerewolfEngine.State;

public record Player(
    string Name,
    PlayerState State,
    TagSet Tags,
    IImmutableList<IRole> Roles,
    IFaction ActiveFaction)
{
    public Player(string name, IRole role) : this(name, PlayerState.Alive, new TagSet(), ImmutableArray.Create(role), role.Faction)
    {
    }
    
    public Player(string name, IRoleBlueprint roleBlueprint) : this(name, roleBlueprint.Build(name))
    {
    }
}