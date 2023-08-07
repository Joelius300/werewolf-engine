namespace WerewolfEngine.State;

public enum GameActionState
{
    AwaitingInput,
    AwaitingTagCollapse,
    AwaitingTagConsequences,
    AwaitingWinConditionEvaluation,
    AwaitingActionGathering,
    GameEnded
}