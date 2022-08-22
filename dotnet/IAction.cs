namespace WerewolfEngine;

public interface IAction
{
    public Player ResponsiblePlayer { get; }
    public IRole ResponsibleRole { get; }
}