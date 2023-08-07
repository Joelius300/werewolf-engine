using WerewolfEngine.Rules;

namespace WerewolfEngine.State;

public record Player(
    string Name,
    PlayerState State,
    TagSet Tags,
    IReadOnlyList<IRole> Roles,
    IFaction ActiveFaction);