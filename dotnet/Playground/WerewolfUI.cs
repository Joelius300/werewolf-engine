using System.Diagnostics;
using WerewolfEngine;
using WerewolfEngine.Actions;
using WerewolfEngine.State;
using WerewolfEngine.WerewolfSampleImpl;

namespace Playground;

// very simple console app UI
public class WerewolfUI
{
    private readonly Game _game;

    public WerewolfUI(Game game) => _game = game;

    public void Run()
    {
        Console.WriteLine($"Players: {string.Join(", ", _game.State.Players.Select(p => p.Name))}");
        Console.WriteLine();
        Console.WriteLine($"Round {_game.State.Round} - {_game.State.Phase}");
        while (_game.State.State != GameActionState.GameEnded)
        {
            var prevState = _game.State;
            _game.Advance(GetInput(_game.GetCurrentInputRequest()));
            var currState = _game.State;

            if (prevState.Phase != currState.Phase)
            {
                Console.WriteLine();
                Console.WriteLine($"Round {_game.State.Round} - {_game.State.Phase}");

                Console.WriteLine((_game.State.Phase == GamePhase.Day ? "This night" : "Today") +
                                  " the following people died: " +
                                  string.Join(", ", PlayersDiedThisNight(prevState, currState)));
                Console.WriteLine();
            }
        }

        Console.WriteLine("Game ended");
        Console.WriteLine($"Winner: {_game.State.Winner?.Name ?? "no one"}");
    }

    // This is a bit clunky, it might be nicer not having to compare the states each time
    // and just being able to get the list of dead players directly from the engine as gathered
    // while it kills (e.g. in the tag consequences, the killed players are added to a list, pretty simple tbh).
    private IEnumerable<string> PlayersDiedThisNight(GameState prevState, GameState currState)
    {
        foreach (var (oldPlayer, newPlayer) in prevState.Players.Zip(currState.Players))
        {
            Debug.Assert(oldPlayer.Name == newPlayer.Name, "oldPlayer.Name == newPlayer.Name");
            if (newPlayer.State == PlayerState.Dead && oldPlayer.State == PlayerState.Alive)
                yield return newPlayer.Name;
        }
    }

    private IInputResponse GetInput(IInputRequest request) =>
        // In Rust, discrimination based on type may not be possible but require some string ID or something.
        // I'll probably want to be able to register a handler anyway instead of a switch case and inside the handler
        // the cast can happen, which should also be possible in Rust IIRC. Maybe that will actually be in flutter then
        // (where the core code will still be rust, not dart, hopefully). The input request could contain
        // the action id of the starting action to discriminate because the action type and the input request type
        // (as well as response type) are 1:1 relationships.

        // UnitInputRequest doesn't work, the type is the discriminator atm so you need a WerewolfInputRequest type,
        // in Rust, if we don't discriminate by type but by id (string for example), then UnitInputRequest works again,
        // actually, just Unit () should work.
        request switch
        {
            WitchInputRequest r => GetWitchInput(r),
            WerewolfInputRequest r => GetWerewolfVote(r),
            DayVotingInputRequest r => GetDayVote(r),
            _ => throw new NotImplementedException($"No handler for type {request.GetType().Name}"),
        };

    private WitchInputResponse GetWitchInput(WitchInputRequest request)
    {
        string? healTarget = null;
        string? killTarget = null;
        if (request.WerewolfTarget is not null)
        {
            Console.WriteLine($"Person to heal: {request.WerewolfTarget.Name}");
            if (request.HealSpellCount > 0)
            {
                if (AskYesNo("Do you want to heal?"))
                    healTarget = request.WerewolfTarget.Name;
            }
        }

        if (request.KillSpellCount > 0)
        {
            killTarget = ReadString("Name of player to kill (empty=none): ");
        }

        return new(healTarget, killTarget);
    }

    private WerewolfInputResponse GetWerewolfVote(WerewolfInputRequest r)
    {
        var name = ReadString("Werewolves voted to kill: ");
        return new(name);
    }

    private DayVotingInputResponse GetDayVote(DayVotingInputRequest r)
    {
        var name = ReadString("Villagers voted to kill: ");
        return new(name);
    }

    private static string? ReadString(string prompt)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();

        return string.IsNullOrWhiteSpace(input) ? null : input;
    }

    private static bool AskYesNo(string prompt)
    {
        Console.Write(prompt + " (y/n) ");
        var yes = Console.ReadKey().Key == ConsoleKey.Y;
        Console.WriteLine();
        return yes;
    }
}