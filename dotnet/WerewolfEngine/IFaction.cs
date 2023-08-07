using WerewolfEngine.State;

namespace WerewolfEngine;

public interface IFaction
{
    public string Name { get; }
    bool HasWon(GameState gameState);
}