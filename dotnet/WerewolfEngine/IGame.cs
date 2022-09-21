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
    IGame Advance(InputResponse inputResponse);
}

public interface IAction<in TInput, out TInputRequest>
    where TInput :
{
    TInputRequest GetInputRequest();
    
    IGame Advance(TInput input);
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
 * I think I'll try to figure out a nice system with an id matching (non-generic) request and a behavior implementing
 * response to avoid downcasting and runtime type checks. We'll see how that goes.
 */

public enum InputType
{
    Player,
    Choice
}

// basically a OneOf<PlayerInputResponse, ...>
public record InputResponse(InputType Type, PlayerInputResponse? Player, ChoiceOfInputResponse? Choice); // in Rust, this would be an enum, no need for Type etc.
public record PlayerInputResponse(string Name);
public record ChoiceOfInputResponse(string Choice);


// OneOf<PlayerInputRequest, ..>
public record InputRequest(InputType Type, PlayerInputRequest? Player, ChoiceOfInputRequest? Choice);

public record BaseInputRequest(string Name, bool Required);

// for now you can't chose the same person twice (by convention)
public record PlayerInputRequest
    (string Name, bool Required, ICollection<string> PlayerPool, int Number) : BaseInputRequest(Name, Required);

public record ChoiceOfInputRequest(string Name, bool Required, ICollection<string> Choices, int Number) : BaseInputRequest(Name, Required);