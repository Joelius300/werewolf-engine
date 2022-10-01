namespace WerewolfEngine.Actions;

public interface IAction<out TInputRequest, in TInputResponse>
    where TInputRequest : IInputRequest
    where TInputResponse : IInputResponse
{
    TInputRequest GetInputRequest(IGame game);
    IGame Do(IGame game, TInputResponse input);
}

public interface IAction
{
    // Player that caused the action or null if multiple. Doesn't really matter, the action just needs to know what it can expect.
    string? ActingPlayer { get; }
    
    /// Types of actions that need to be run before this one
    // That's the only thing we need I believe because most actions have no dependency on others because they are just
    // tagging players and that can happen independently. The dependencies are for when the input request is dependent
    // on the input response of a previous action.
    // A few things that would be nice for the action ordering:
    // - being consistent, e.g. using the same base order if there are no dependencies (alphabetically or whatever)
    // - being able to provide a base order which is just used (with all the actions that are present obv.) as long as no dependency is violated
    IReadOnlyCollection<Type> BeforeActionDependencies { get; }

    /// Get appropriate input request with parameters set according to the state of the game passed as parameter.
    IInputRequest GetInputRequest(IGame game);

    /// Transform the given game with the given input into a new game.
    // The game given here to transform should be the same as the one given when asking for the request (otherwise you
    // might run into trouble) but storing a reference to a game is probably a bad idea because basically all modifications
    // of the game result in a new instance. Either we always recreate all actions and make sure they point to the new
    // instance (we need to do that for everything that holds a reference, not just actions). It would be safer to use
    // but more complex and difficult to implement. Either way, changes to a game between handing out an input request
    // and applying the returned response, invalidate the action and are to be prevented somehow.
    IGame Do(IGame game, IInputResponse input);
}