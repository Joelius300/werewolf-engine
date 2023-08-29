using WerewolfEngine.Actions;
using WerewolfEngine.WerewolfSampleImpl;

namespace Playground;

// very simple console app UI
public class WerewolfUI
{
    public IInputResponse GetInput(IInputRequest request) =>
        // In Rust, discrimination based on type may not be possible but require some string ID or something.
        // I'll want to be able to register a handler anyway instead of a switch case and inside the handler
        // the cast can happen, which should also be possible in Rust IIRC. The input request could contain
        // the action id of the starting action to discriminate because the action type and the input request type
        // (as well as response type) are 1:1 relationships.
        request switch
        {
            WitchInputRequest r => GetWitchInput(r),
            // UnitInputRequest doesn't work, the type is the discriminator atm so you need a WerewolfInputRequest type,
            // in Rust, if we don't discriminate by type but by id (string for example), then UnitInputRequest works again,
            // actually, just Unit () should work.
            _ => throw new NotImplementedException(),
        };

    private WitchInputResponse GetWitchInput(WitchInputRequest request)
    {
        throw new NotImplementedException();
    }
}