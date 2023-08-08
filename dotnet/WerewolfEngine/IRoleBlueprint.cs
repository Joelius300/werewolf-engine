namespace WerewolfEngine;

public interface IRoleBlueprint
{
    IRole Build(string playerName);
}