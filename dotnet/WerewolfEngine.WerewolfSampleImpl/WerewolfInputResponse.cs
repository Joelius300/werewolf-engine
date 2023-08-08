using WerewolfEngine.Actions;

namespace WerewolfEngine.WerewolfSampleImpl;

public record WerewolfInputResponse(string? Target) : IInputResponse;