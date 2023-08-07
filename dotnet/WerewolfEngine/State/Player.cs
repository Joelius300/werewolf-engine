using WerewolfEngine.Rules;

namespace WerewolfEngine.State;

public record Player(
    string Name,
    PlayerState State,
    TagSet Tags,
    IReadOnlyList<IRole> Roles,
    IFaction ActiveFaction);
    
// TODO constructor or factory method for simple initialization of player with just a name and a role.