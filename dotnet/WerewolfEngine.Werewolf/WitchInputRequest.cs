using WerewolfEngine.Actions;

namespace WerewolfEngine.Werewolf;

public record WitchInputRequest(int HealSpellCount, int KillSpellCount) : IInputRequest;