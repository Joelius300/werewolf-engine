using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine.WerewolfSampleImpl;

public class GuardianAction : BaseAction<GuardianRole, GuardianInputRequest, GuardianInputResponse>
{
    public GuardianAction(RoleAccessor<GuardianRole> originRole) : base(originRole)
    {
    }

    public override GuardianInputRequest GetInputRequest(GameState game) =>
        new(game.Query(OriginRole).LastProtectedPlayer);

    public override GameState Transform(GameState game, GuardianInputResponse input)
    {
        var lastProtectedPlayer = game.Query(OriginRole).LastProtectedPlayer;
        if (input.ProtectedPlayer == lastProtectedPlayer)
            throw new ArgumentException($"Cannot protect the same player ('{lastProtectedPlayer}') again.");

        game.CheckAlive(input.ProtectedPlayer);

        game = game.TagPlayer(input.ProtectedPlayer, GuardianRole.ProtectedByGuardian);
        game = game.UpdateRole(OriginRole, r => r with {LastProtectedPlayer = input.ProtectedPlayer});
        return game;
    }
}