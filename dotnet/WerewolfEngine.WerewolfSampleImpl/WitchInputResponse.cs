using WerewolfEngine.Actions;

namespace WerewolfEngine.WerewolfSampleImpl;

public record WitchInputResponse(string? HealTargetName, string? KillTargetName) : IInputResponse;