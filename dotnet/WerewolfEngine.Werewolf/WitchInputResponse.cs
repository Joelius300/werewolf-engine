using WerewolfEngine.Actions;

namespace WerewolfEngine.Werewolf;

public record WitchInputResponse(string? HealTargetName, string? KillTargetName) : IInputResponse;