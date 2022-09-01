namespace WerewolfEngine;

public class WerewolfNightAction : BaseAction<WerewolfInputRequest, WerewolfInputSubmission>, IEquatable<WerewolfNightAction>
{
    public WerewolfNightAction(Player responsiblePlayer, IRole responsibleRole) : base(responsiblePlayer,
        responsibleRole)
    {
    }

    public override WerewolfInputRequest GetInputRequest() => new();


    protected override Game Transform(Game game) => game.TagPlayer(Input!.Target, CreateTag(Werewolf.KilledByWerewolf));
    
    // For Werewolf action, we only care about the type as there can only be one Werewolf action per Night.
    public bool Equals(WerewolfNightAction? other) => other?.GetType() == GetType();
    public override bool Equals(object? obj) => Equals(obj as WerewolfNightAction);
    public override int GetHashCode() => 0;
    public static bool operator ==(WerewolfNightAction? left, WerewolfNightAction? right) => Equals(left, right);
    public static bool operator !=(WerewolfNightAction? left, WerewolfNightAction? right) => !Equals(left, right);
}