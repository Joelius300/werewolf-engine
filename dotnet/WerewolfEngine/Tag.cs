namespace WerewolfEngine;

public record Tag(string Identifier)
{
    // public IReadyAction? CausingAction { get; }

    public override string ToString() => $"'{Identifier}'";
}