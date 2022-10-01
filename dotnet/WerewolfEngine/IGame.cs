using WerewolfEngine.Actions;

namespace WerewolfEngine;

public interface IGame
{
    IInputRequest GetCurrentInputRequest();
    IGame Advance(IInputResponse inputResponse);
}
