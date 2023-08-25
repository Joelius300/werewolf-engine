using WerewolfEngine.Actions;

namespace WerewolfEngine.WerewolfSampleImpl;

public record GuardianInputResponse(string ProtectedPlayer) : IInputResponse;