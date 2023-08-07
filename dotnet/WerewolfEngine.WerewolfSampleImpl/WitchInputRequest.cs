using WerewolfEngine.Actions;

namespace WerewolfEngine.WerewolfSampleImpl;

public record WitchInputRequest(int HealSpellCount, int KillSpellCount) : IInputRequest;