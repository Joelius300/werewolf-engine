using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

public class DayVotingAction : BaseAction<GodRole, DayVotingInputRequest, DayVotingInputResponse>
{
    public DayVotingAction() : base(GodRole.Accessor)
    {
    }

    public override DayVotingInputRequest GetInputRequest(GameState game) => new();

    public override GameState Transform(GameState game, DayVotingInputResponse input)
    {
        if (input.VotedOutPlayer is null)
            return game;

        game.CheckAlive(input.VotedOutPlayer);

        return game.TagPlayer(input.VotedOutPlayer, GodRole.KilledByVillage);
    }
}

public record DayVotingInputRequest() : IInputRequest;
public record DayVotingInputResponse(string? VotedOutPlayer) : IInputResponse;