using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace WerewolfEngine;

public sealed class PlayerCircle : IImmutableList<Player>
{
    private readonly IImmutableList<Player> _players;
    private readonly IReadOnlyDictionary<string, int> _nameLookup;

    private PlayerCircle(IImmutableList<Player> players)
    {
        _players = players ?? throw new ArgumentNullException(nameof(players));

        // init name lookup and check unique names in one go
        Dictionary<string, int> dict = new();
        for (int i = 0; i < players.Count; i++)
        {
            if (!dict.TryAdd(players[i].Name, i))
                throw new ArgumentException("All players have to have unique names.");
        }

        _nameLookup = new ReadOnlyDictionary<string, int>(dict);
    }

    public PlayerCircle(IEnumerable<Player> players) : this(players?.ToImmutableList()!)
    {
    }

    public int Count => _players.Count;

    // circular access by index to work with neighbours
    public Player this[int index] => _players[index % Count];
    public Player this[string name] => NameLookup(name);

    private Player NameLookup(string name)
    {
        if (!_nameLookup.TryGetValue(name, out int index))
            throw new ArgumentException($"No Player with the name '{name}'.");

        return _players[index];
    }

    public int IndexOf(Player item, int index, int count, IEqualityComparer<Player>? equalityComparer) =>
        _players.IndexOf(item, index, count, equalityComparer);

    public int LastIndexOf(Player item, int index, int count, IEqualityComparer<Player>? equalityComparer) =>
        _players.LastIndexOf(item, index, count, equalityComparer);


    // @formatter:off
    public PlayerCircle Add(Player value) => new(_players.Add(value));
    public PlayerCircle AddRange(IEnumerable<Player> items) => new(_players.AddRange(items));
    public PlayerCircle Clear() => new(_players.Clear());
    public PlayerCircle Insert(int index, Player element) => new(_players.Insert(index, element));
    public PlayerCircle InsertRange(int index, IEnumerable<Player> items) => new(_players.InsertRange(index, items));
    public PlayerCircle Remove(Player value, IEqualityComparer<Player>? equalityComparer) => new(_players.Remove(value, equalityComparer));
    public PlayerCircle RemoveAll(Predicate<Player> match) => new(_players.RemoveAll(match));
    public PlayerCircle RemoveAt(int index) => new(_players.RemoveAt(index));
    public PlayerCircle RemoveRange(IEnumerable<Player> items, IEqualityComparer<Player>? equalityComparer) => new(_players.RemoveRange(items, equalityComparer));
    public PlayerCircle RemoveRange(int index, int count) => new(_players.RemoveRange(index, count));
    public PlayerCircle Replace(Player oldValue, Player newValue, IEqualityComparer<Player>? equalityComparer) => new(_players.Replace(oldValue, newValue, equalityComparer));
    public PlayerCircle SetItem(int index, Player value) => new(_players.SetItem(index, value));
    public PlayerCircle UpdatePlayer(string name, Player newValue) => SetItem(_nameLookup[name], newValue);
    public PlayerCircle UpdatePlayer(Player updatedPlayer) => UpdatePlayer(updatedPlayer.Name, updatedPlayer);


    IImmutableList<Player> IImmutableList<Player>.Add(Player value) => Add(value);
    IImmutableList<Player> IImmutableList<Player>.AddRange(IEnumerable<Player> items) => AddRange(items);
    IImmutableList<Player> IImmutableList<Player>.Clear() => Clear();
    IImmutableList<Player> IImmutableList<Player>.Insert(int index, Player element) => Insert(index, element);
    IImmutableList<Player> IImmutableList<Player>.InsertRange(int index, IEnumerable<Player> items) => InsertRange(index, items);
    IImmutableList<Player> IImmutableList<Player>.Remove(Player value, IEqualityComparer<Player>? equalityComparer) => Remove(value, equalityComparer);
    IImmutableList<Player> IImmutableList<Player>.RemoveAll(Predicate<Player> match) => RemoveAll(match);
    IImmutableList<Player> IImmutableList<Player>.RemoveAt(int index) => RemoveAt(index);
    IImmutableList<Player> IImmutableList<Player>.RemoveRange(IEnumerable<Player> items, IEqualityComparer<Player>? equalityComparer) => RemoveRange(items, equalityComparer);
    IImmutableList<Player> IImmutableList<Player>.RemoveRange(int index, int count) => RemoveRange(index, count);
    IImmutableList<Player> IImmutableList<Player>.Replace(Player oldValue, Player newValue, IEqualityComparer<Player>? equalityComparer) => Replace(oldValue, newValue, equalityComparer);
    IImmutableList<Player> IImmutableList<Player>.SetItem(int index, Player value) => SetItem(index, value);
    // @formatter:on

    public IEnumerator<Player> GetEnumerator() => _players.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _players).GetEnumerator();
}