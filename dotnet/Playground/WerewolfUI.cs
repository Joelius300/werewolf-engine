using WerewolfEngine.Actions;
using WerewolfEngine.WerewolfSampleImpl;

namespace Playground;

// very simple console app UI
public class WerewolfUI
{
    public IInputResponse GetInput(IInputRequest request) =>
        request switch
        {
            WitchInputRequest r => GetWitchInput(r),
            // UnitInputRequest doesn't work, the type is the discriminator atm so you need a WerewolfInputRequest type
            _ => throw new NotImplementedException(),
        };

    private WitchInputResponse GetWitchInput(WitchInputRequest request)
    {
        throw new NotImplementedException();
    }
}