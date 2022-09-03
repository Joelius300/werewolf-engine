namespace WerewolfEngine;

// so in Rust these will be actually easier I believe because WerewolfInputRequest
// would just be a (unit?) struct. Then in the actual app I could either implement a trait
// for this request which would return some sort of generic struct which can model any kind of
// request (or I could include that generic request model in the engine as well? then I could just create that method
// on IInputRequest) or in the dart app I could switch on the type and create the widget according to that.
// Keep in mind that some input requests will require data, e.g. the witch's request will need the available spells.
public record WerewolfInputRequest : IInputRequest;