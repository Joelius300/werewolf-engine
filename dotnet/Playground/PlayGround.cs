using System.Text.Json;
using System.Text.Json.Serialization;
using WerewolfEngine;
using WerewolfEngine.Actions;

namespace Playground;

public abstract class PlayGround
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters =
        {
            new JsonTypeConverter(),
            new JsonTagConverter(),
            new JsonStringEnumConverter(),
            new JsonActualTypeConverter<IRole>(),
            new JsonActualTypeConverter<IFaction>(),
            new JsonActualTypeConverter<IAction>(),
            new JsonGenericActionConverterFactory()
        },
    };

    public abstract void Play();

    // this is just to print the whole game as json with their actual types
    protected string Serialize(object obj) => JsonSerializer.Serialize(obj, JsonOptions);

    protected void PrintRequest(IInputRequest request)
    {
        var type = request.GetType();
        Console.WriteLine($"Request of type '{type.FullName}':");
        Console.WriteLine(Serialize(request));
        Console.WriteLine();
    }

    protected void SubmitInput(IGame game, IInputResponse response)
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
}