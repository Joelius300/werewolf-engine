using System.Collections.Immutable;
using WerewolfEngine.Actions;

namespace WerewolfEngine.State;

public record GameState(
    GamePhase Phase,
    int Round,
    GameActionState State,
    IFaction? Winner,
    IImmutableList<Player> Players,
    IAction? CurrentAction,
    IImmutableList<IAction> NextActions);