using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine.WerewolfSampleImpl;

public record WitchInputRequest(Player? WerewolfTarget, int HealSpellCount, int KillSpellCount) : IInputRequest;