using WerewolfEngine.Actions;
using WerewolfEngine.State;

namespace WerewolfEngine;

public interface IGame
{
    GameState State { get; }
    IInputRequest? GetCurrentInputRequest();
    void Advance(IInputResponse inputResponse);
}
