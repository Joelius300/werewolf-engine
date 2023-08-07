using WerewolfEngine.Actions;

namespace WerewolfEngine;

public interface IGame
{
    IInputRequest? GetCurrentInputRequest();
    void Advance(IInputResponse inputResponse);
}
