using System.Text.Json;
using System.Text.Json.Serialization;
using WerewolfEngine;
using WerewolfEngine.Actions;
using WerewolfEngine.Rules;
using WerewolfEngine.State;
using WerewolfEngine.WerewolfSampleImpl;

SampleImpl();

void Actions()
{
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
 *
 * Update, we're now working on the downcast solution.
 */
}

void Rules()
{
    List<Rule> rules = new();

    // Werewolf
    const string KilledByWerewolf = "killed_by_werewolf";
    rules.Add(new Rule(new TagSet(KilledByWerewolf), new TagSet(MasterTag.Killed), true));

    // Witch
    const string HealedByWitch = "healed_by_witch";
    const string KilledByWitch = "killed_by_witch";
    rules.Add(new Rule(new TagSet(HealedByWitch), new TagSet(), true));
    rules.Add(new Rule(new TagSet(KilledByWitch), new TagSet(MasterTag.Killed), true));
    rules.Add(new Rule(new TagSet(KilledByWerewolf, HealedByWitch), new TagSet(HealedByWitch), false));
    // no Rule for killed_by_werewolf and killed_by_witch since that can't happen in game (well that's up to the GM..)
    // in the same way healed_by_witch and killed_by_witch don't work together either (define either error or resolve)

    // Guardian
    const string ProtectedByGuardian = "protected_by_guardian";
    rules.Add(new Rule(new TagSet(ProtectedByGuardian), new TagSet(), true));
    rules.Add(new Rule(new TagSet(KilledByWerewolf, ProtectedByGuardian), new TagSet(ProtectedByGuardian), false));
    rules.Add(new Rule(new TagSet(KilledByWitch, ProtectedByGuardian), new TagSet(ProtectedByGuardian), false));
    rules.Add(new Rule(new TagSet(HealedByWitch, ProtectedByGuardian), new TagSet(ProtectedByGuardian), false));


// UPDATE 18.09.22 8deb974
// There is now collision handling in place which checks if all matching rules do the same thing directly, or if that's
// not the case, it recursively collapses each branch and checks if they all result in the same end set. This is a bit
// different than trying to find an order to apply the rules in but I think much better even though more computationally
// intensive. One could of course consider doing both to try to improve performance but oh boy that's not relevant right now.
// But since there are days where nothing quite gets me going like some nice trees and other data structures, I might later
// on try to come up with a tree based system for rule resolving which would not only be very sexy but most likely more performant
// as well. In such a system the algorithm would only need to traverse a branch until it can connect it to an already existing node.
// You could also store the explicit rules in a tree and just traverse until you can't anymore before even considering
// non-explicit ones but I'm not sure if that's any better than just having the dict, fetching the potentially matching rules
// and looking for a good one until you don't fine one. For the non-explicit ones you wouldn't use an existing tree for
// traversal but instead build one when resolving conflicts and use it as a kind of cache to avoid having to collapse 
// each branch until completion. Another possibility would be to build the tree continually as new sets are collapsed
// as a cache which would speed up execution over time. You could even do that for every tag combination beforehand but 
// just with 50 tags that's already 1125899906842624 (=2^50) combinations. Also in this case I'm saying tree but you could
// also just store the known end result for every tag set you handle on your path to the fully collapsed set. Anyway, the
// current system works just fine and performance isn't anything to worry about for now.

// OLD NOTE before collision handling
// here we have a case where we see a non-explicit rule from 2 to 1. Note that in the current implementation, this
// would block all other non-explicit rules 2->1 from being added. This might be too limiting because then you can't
// define healed + killed for other roles in the same way. One way to solve this would be to not block these collisions
// and instead only do the recovery when there are more than one rule of the same priority that match. In that case you
// could for example try to find an order of applying all (e.g. try one, check if the next one also matches, check if
// the next also matches, etc. and backtrack until you find an order that manages to apply all of them or throws if not).
// Imagine the case of killed_by_werewolf, killed_by_witch and protected_by_guardian. You could create two rules for the
// protection namely killed_by_werewolf + protected_by_guardian -> protected_by_guardian and 
// killed_by_witch + protected_by_guardian -> protected_by_guardian. The algorithm would then find that first applying
// rule one would reduce it to killed_by_witch + protected_by_guardian and that still matches the second rule which could
// then be applied as well. I think that would allow for much more flexibility when defining the rules and prevent a lot
// of redundancies. Additionally, if all else fails, just before giving up, you could just apply one, keep going and see
// where you end up. Then backtrack and do the same for all the other colliding rules as well. If they all end up on the
// same results tagset in the end, just take it and run with it (the first one I guess).

// also old notes but both still relevant

// When we get to attributions and
// meta-data inside the tags, e.g. who is responsible for the killed tag on a player, this part would need to be revisited
// to make sure there is a sensible logic for determining who is or are used as responsible actions/roles for the killed
// tag. Maybe since that's a thing that will come up multiple times, have some utilities for things like creating rules
// from a tagset to another where one rule is created for all possible (r-)combinations of those tags in the first set.

// This also brings me to the idea of a CombineMetaData flag for a rule. This flag would indicate that when collapsing
// a TagSet, the meta-data of all the tags that aren't also in the To set (those can still be copied 1:1) are combined
// into a list of responsible actions, etc. and added to all new tags in the To set (those who weren't in From and thus
// couldn't just copy the meta-data).
// These would be used for all the killed rules. I imagine a method, that takes a set
// of all tags in the game, picks out only the non-master-tags who start with "killed", creates all combinations with
// them, creates an explicit rule for each combination that just leads to "killed" with the CombineMetaData flag set.

    RuleSet ruleSet = new(rules, Enumerable.Empty<string>());
    TagSet tags = new(KilledByWerewolf);
    Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just killed");
    tags = new(KilledByWitch);
    Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just killed");
    tags = new(KilledByWerewolf, HealedByWitch);
    Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just nothing");
    tags = new(KilledByWerewolf, ProtectedByGuardian);
    Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just nothing");
    tags = new(KilledByWerewolf, HealedByWitch, ProtectedByGuardian);
    Console.WriteLine($"Collapse {tags} to {ruleSet.Collapse(tags)}: should be just nothing");
}

void SampleImpl()
{
    var players = new PlayerCircle(new[]
    {
        new Player("P1-villager", new VillagerRole.Blueprint()),
        new Player("P2-witch", new WitchRole.Blueprint(1, 1)),
        new Player("P3-werewolf", new WerewolfRole.Blueprint()),
    });

    var rules = new RuleSet(new[]
        {
            // Werewolf
            new Rule(new TagSet(WerewolfRole.KilledByWerewolves), new TagSet(MasterTag.Killed), true),
            // Witch
            new Rule(new TagSet(WitchRole.HealedByWitch), new TagSet(), true),
            new Rule(new TagSet(WitchRole.KilledByWitch), new TagSet(MasterTag.Killed), true),
            new Rule(new TagSet(WerewolfRole.KilledByWerewolves, WitchRole.HealedByWitch),
                new TagSet(WitchRole.HealedByWitch), false)
            // no Rule for killed_by_werewolf and killed_by_witch since that can't happen in game (well that's up to the GM..)
            // in the same way healed_by_witch and killed_by_witch don't work together either (define either error or resolve)
        },
        new[]
        {
            WerewolfRole.RoleName,
            WitchRole.RoleName
        });

    var game = new Game(players, rules);


    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters =
        {
            new JsonTypeConverter(),
            new JsonStringEnumConverter(),
            new JsonConcreteTypeConverter<IRole>(),
            new JsonConcreteTypeConverter<IFaction>(),
            new JsonConcreteTypeConverter<IAction>(),
            new JsonGenericActionConverterFactory()
        },
        // TODO actual deep polymorphic serialization, e.g. role isn't serialized polymorphically
    };
    string Serialize(object obj) => JsonSerializer.Serialize(obj, jsonOptions);

    void PrintRequest(IInputRequest request)
    {
        var type = request.GetType();
        Console.WriteLine($"Request of type '{type.FullName}':");
        Console.WriteLine(Serialize(request));
        Console.WriteLine();
    }

    void SubmitInput(IInputResponse response)
    {
        var type = response.GetType();
        Console.WriteLine($"Inputting response of type '{type.FullName}':");
        Console.WriteLine(Serialize(response));
        Console.WriteLine();

        game.Advance(response);

        Console.WriteLine("Game state after input:");
        Console.WriteLine(Serialize(game.State));
        Console.WriteLine();
    }

    // Scripted actions to simulate example a from concept

    Console.WriteLine("Initial game state");
    Console.WriteLine(Serialize(game.State));
    Console.WriteLine();

    PrintRequest(game.GetCurrentInputRequest());

    SubmitInput(new WerewolfInputResponse("P1-villager"));

    PrintRequest(game.GetCurrentInputRequest());

    SubmitInput(new WitchInputResponse(null, "P3-werewolf"));

    Console.WriteLine("Final game state");
    Console.WriteLine(Serialize(game.State));
}

class JsonTypeConverter : JsonConverter<Type>
{
    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.FullName);
    }
}

class JsonConcreteTypeConverter<T> : JsonConverter<T>
    where T : class
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

class JsonGenericActionConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        return typeToConvert.GetGenericTypeDefinition() == typeof(IAction<,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter?) Activator.CreateInstance(
            typeof(JsonGenericActionConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()));
    }
}

class JsonGenericActionConverter<TInputRequest, TInputResponse> : JsonConcreteTypeConverter<
    IAction<TInputRequest, TInputResponse>>
    where TInputRequest : IInputRequest
    where TInputResponse : IInputResponse
{
}