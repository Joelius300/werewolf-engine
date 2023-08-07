using WerewolfEngine.State;

namespace WerewolfEngine.Actions;

public interface IAction<out TInputRequest, in TInputResponse>
    where TInputRequest : IInputRequest
    where TInputResponse : IInputResponse
{
    // Instead of multiple helper functions on an action that
    // fetch things like which player was selected to kill,
    // just have this one input request that contains all that info
    TInputRequest GetInputRequest(GameState game);
    
    GameState Transform(GameState game, TInputResponse input);

    IAction ToPlainAction();  // since IAction and IAction<U, V> aren't compatible type right now
}

public interface IAction
{
    public Type InputRequestType { get; }
    public Type InputResponseType { get; }
    
    /// Get appropriate input request with parameters set according to the state of the game passed as parameter.
    IInputRequest GetInputRequest(GameState game);

    /// Transform the given game state with the given input into a new game.
    // The game given here to transform must be the same as the one given when asking for the request (otherwise the
    // action might not be possible to execute or cause inconsistencies).
    // but storing a reference to a game is probably a bad idea because basically all modifications
    // of the game result in a new instance. Either we always recreate all actions and make sure they point to the new
    // instance (we need to do that for everything that holds a reference, not just actions). It would be safer to use
    // but more complex and difficult to implement. Either way, changes to a game between handing out an input request
    // and applying the returned response, invalidate the action and are to be prevented somehow.
    GameState Transform(GameState game, IInputResponse input);
}