namespace WerewolfEngine;

public interface IReadyAction : IAction
{
    Game Transform(Game game);
}