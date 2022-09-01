namespace WerewolfEngine;

public class Werewolf : IRole
{
    public const string KilledByWerewolf = "killed_by_werewolf";
    public Player Player { get; }

    public Werewolf(Player player)
    {
        Player = player;
    }

    public bool RegisterNightAction(Game game, IList<IPendingAction<IInputRequest, IInputSubmission>> actions)
    {
        if (actions.Any(a => a.GetType() == typeof(WerewolfNightAction)))
        {
            return false;
        }
        
        // TODO order matters (technically it doesn't as they will just tag the players and then the tags will
        // resolve at the end of the night but since the engine currently dictates what input is requested one at a time
        // it would make sense for the request to be in a natural order). Some other ideas include:
        /* - don't have the engine dictate which action to make ready, instead just expose the actions that need to be
         *   readied. However, this does have the disadvantage that it's harder to update the actions queue during the
         *   night (e.g. when the lucky bastard is attacked).
         * For ordering the actions, I already wrote something in Program.cs with "before" and "after" dependencies.
         */
        actions.Add(new WerewolfNightAction(Player, this));

        return true;
    }
}