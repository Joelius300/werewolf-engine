using System.Collections;

namespace WerewolfEngine;

public class MasterTag : Tag
{
    public static MasterTag Killed { get; } = new("Killed");
    
    // has to be internal and cannot be sealed because a RoleTag for example which has a role parameter
    // will be a subtype of MasterTag, only allowed to be constructed from the engine devs _and_ has to
    // override the equality because you could have multiple role_added tags with different roles e.g. falling
    // in love as well as turning into a werewolf.
    internal MasterTag(string identifier) : base(identifier)
    {
    }

    public static IEnumerable<MasterTag> AllMasterTags()
    {
        yield return Killed;
    }
}