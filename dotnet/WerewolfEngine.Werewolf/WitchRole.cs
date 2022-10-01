namespace WerewolfEngine.Werewolf;

public record WitchRole(int HealSpellCount, int KillSpellCount) : IRole
{
    public const string KilledByWitch = "killed_by_witch";
    public const string HealedByWitch = "healed_by_witch";
    
    
}