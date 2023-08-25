using WerewolfEngine.Actions;

namespace WerewolfEngine.WerewolfSampleImpl;

public record GuardianInputRequest(string? LastProtectedPlayer) : IInputRequest;