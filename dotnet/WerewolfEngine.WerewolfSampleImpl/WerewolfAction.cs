using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine.WerewolfSampleImpl;

public class WerewolfAction : BaseAction<WerewolfRole, UnitInputRequest, WerewolfInputResponse>
{
    public WerewolfAction(RoleAccessor<WerewolfRole> originRole) : base(originRole)
    {
    }

    public override UnitInputRequest GetInputRequest(GameState game) => new();

    public override GameState Transform(GameState game, WerewolfInputResponse input)
    {
        if (input.Target is null)
            return game;

        game.CheckAlive(input.Target);

        return game.TagPlayer(input.Target, WerewolfRole.KilledByWerewolves);
    }
}