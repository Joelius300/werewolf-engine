using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

public class DayVotingAction : BaseAction<UnitInputRequest, DayVotingInputResponse>
{
    public override UnitInputRequest GetInputRequest(GameState game) => new();

    public override GameState Transform(GameState game, DayVotingInputResponse input)
    {
        throw new NotImplementedException();
        // assign "killed_by_village" tag. Henker can also have tags that work together with that.
    }
}

public record DayVotingInputResponse(string? VotedOutPlayer) : IInputResponse
{
}