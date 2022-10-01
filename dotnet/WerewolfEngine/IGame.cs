using WerewolfEngine.Rules;

namespace WerewolfEngine;

/* If I've learned anything about creating an application from the ground up, especially in rust,
 * which is the target language after this prototype, it's that you first need to model the state
 * of the application as precisely as possible with the tools the type system gives you.
 * I'd imagine that would mean working with generics a ton instead of dynamic stuff.
 *
 * However:
 * - there will be roles that can add actions to the queue during the game
 * - unless working with a pre-processor similar to rusts macros, you could not know at compile time which input will
 *   be requested once this one is accepted and processed.
 *
 * I think that means I should probably stick to non-generics for now and try my best to work around dynamic stuff
 * because a) it can be hacky b) rust doesn't have a managed runtime with reflection etc. Maybe it would be better to
 * have a general input request and submission with enough flexibility to account for all types of IO that exists in this
 * game, then each action, which can still be generic with concrete types, has to ensure that the concrete input
 * and output types are able to construct to or from the general ones. That should work I think :)
 */

public interface IGame
{
    IGame Advance(IInputResponse inputResponse);

    IPlayer GetPlayer(string name);
    IGame UpdatePlayer(IPlayer player);
    IGame UpdatePlayer(string name, Func<IPlayer, IPlayer> updater);
    IGame TagPlayer(string name, Tag tag);
}

public interface IPlayer
{
    IRole Role { get; }

    T GetRole<T>();
    IPlayer Tag(Tag tag);
    IPlayer UpdateRole<T>(Func<T, T> updater);
}

public interface IRole
{
    // something something get action (see L127 in the messy dotnet Program.cs for ideas)
}

public record WitchRole(int HealSpellCount, int KillSpellCount) : IRole
{
    public const string HealedByWitch = "healed_by_witch";

    public WitchRole UpdateHealSpellCount(int relative) => this with {HealSpellCount = HealSpellCount + relative};
}

/* An action should provide a way to obtain an input request that the UI or API or whatever can use to build the request
 * for the user. With the input from the user, the UI or API or whatever can then create the appropriate response object.
 * For this to work and be pleasant to use, the following things should be possible:
 * - the Witch's action should be able to return something like this: new WitchInputRequest(HealSpellCount=0, KillSpellCount=1)
 * - the Witch's action should be able to receive something like this: WitchInputResponse { HealSpellTarget=null, KillSpellTarget="Joel" }
 *
 * Now, the important questions that need answering are:
 * - how does the UI know what the engine needs to know and build it's controls appropriately?
 *   Matching the type isn't possible but matching an identifier would be, something like "witch_input". Or go full generic
 *   and define a data structure that is capable of formulating all possible input requests and the UI engine would just
 *   turn that into user controls. That would also mean generic responses.
 * - how does the engine receive the input if the input and action types aren't known at compile time? unlike an FSM, you
 *   don't know all the possible successions of transitions beforehand. In an FSM it's well defined what transitions are
 *   possible in a given state and you know that after A -> B, only -> C is possible. However, in our case, after A -> B,
 *   some event could have been triggered, a new action added and suddenly the next transition is -> D instead of -> C
 *   like before. That means you don't know what comes after A -> B until you do the transition A -> B.
 *   In the C# case, we could just take any IInputResponse and check is WitchInputResponse and if not reject it. However,
 *   in Rust that isn't possible AFAIK because there are no interfaces or runtime type checks. You could accept an
 *   impl InputResponse I think but I'm not sure how that would help. The problem is that I don't care about behaviour,
 *   I care about the data contained in the struct and that is specific to every struct and thus cannot be generalized in
 *   the InputResponse trait. IIRC there are two possible solutions to this. Either you use Any and do actually downcast
 *   https://stackoverflow.com/questions/33687447/how-to-get-a-reference-to-a-concrete-type-from-a-trait-object
 *   https://stackoverflow.com/questions/27892375/can-i-do-type-introspection-with-trait-objects-and-then-downcast-it
 *   or you could pivot and implement the transformation directly on the response. That would mean you have a PendingAction
 *   which has to deal with the first problem (id or overly generic whatever) and then the UI can determine from that
 *   PendingAction, which type of ActionResponse it should instantiate and return to the engine. This ActionResponse would
 *   already implement the transformation on the game according to the given inputs and thus you could work with the trait
 *   object because it's just a question of behaviour, not data.
 *
 * I think I'll try to figure out a nice system with an id matching (non-generic) request (requires downcast but outside
 * of the engine) and a behavior implementing response to avoid downcasting and runtime type checks. We'll see how that goes.
 *
 * Update, I don't really like this either.. I believe that the action should be able to decide what happens with the
 * input of the user. As it stands right now, the developer of the UI, API, whatever, is responsible for instantiating
 * the response object and by extension needs to know the appropriate type. If in the future there are multiple versions
 * of the witch but with the same input possibilities, then you would still need to create two new types for request and response.
 * It would be much nicer to keep the request and response just as data carriers even though that means downcasting is
 * required. In C# that's no big deal. In Rust it seems like it would work as well even though it's discouraged for good
 * reason. One possibility I see, even though maybe not for the beginning, is to build the whole game with macros which
 * would allow the engine to build a strongly typed enum with all the possible request and responses so you could match
 * them without a downcast (that also has some lifetime limitation that hopefully aren't impactful for my use-case but still).
 */

public interface IInputRequest { }
public interface IInputResponse { }



// It's clear by convention that the appropriate result this transforms into is a WitchInputResponse.
public record WitchInputRequest(int HealSpellCount, int KillSpellCount) : IInputRequest;

public record WitchInputResponse(string? HealTargetName, string? KillTargetName) : IInputResponse;

public class WitchAction : Action<WitchInputRequest, WitchInputResponse>
{
    private new string ActingPlayer => base.ActingPlayer!;

    public WitchAction(string witch) : base(witch)
    {
    }
    
    public override WitchInputRequest GetInputRequest(IGame game)
    {
        var witch = game.GetPlayer(ActingPlayer);
        var role = witch.GetRole<WitchRole>();

        return new WitchInputRequest(role.HealSpellCount, role.KillSpellCount);
    }

    public override IGame Do(IGame game, WitchInputResponse input)
    {
        if (input.HealTargetName is not null)
        {
            game = game.UpdatePlayer(game.GetPlayer(input.HealTargetName).Tag(WitchRole.HealedByWitch));
            // or
            game = game.UpdatePlayer(input.HealTargetName, p => p.Tag(WitchRole.HealedByWitch));
            // or
            game = game.TagPlayer(input.HealTargetName, WitchRole.HealedByWitch);

            game.UpdatePlayer(ActingPlayer, p => p.UpdateRole<WitchRole>(r => r.UpdateHealSpellCount(-1)));
        }

        if (input.KillTargetName is not null)
        {
            // same here
        }

        return game;
    }
}

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

public abstract class Action<TInputRequest, TInputResponse> : IAction<TInputRequest, TInputResponse>, IAction
    where TInputRequest : IInputRequest
    where TInputResponse : IInputResponse
{
    public string? ActingPlayer { get; }

    protected Action(string? actingPlayer)
    {
        ActingPlayer = actingPlayer;
    }
    
    public abstract TInputRequest GetInputRequest(IGame game);
    public abstract IGame Do(IGame game, TInputResponse input);

    IInputRequest IAction.GetInputRequest(IGame game) => GetInputRequest(game);
    
    IGame IAction.Do(IGame game, IInputResponse input)
    {
        if (input is TInputResponse response)
            return Do(game, response);

        throw new ArgumentException($"The input response has an incompatible type. Requires {typeof(TInputResponse).Name}", nameof(input));
    }
}
