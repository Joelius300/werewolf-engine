# Werewolf engine

This is an unfinished attempt at a Werewolf engine according to [these rules](https://joelius300.github.io/werewolf-guide/) built for a very specific purpose that is a GM assistant app. This GM assistant app will be built afterwards using [flutter_rust_bridge](https://github.com/fzyzcjy/flutter_rust_bridge) and has the following tasks:

- make it hard to forget things
- keep track of / alert at edge cases
- keep history of events

## Design

In order to achieve the goals I set myself for the assistant app, I will be trying to adhere to these design guidelines:

- All state is immutable. Advancing the game results in a new game state as opposed to modifying the current one.
- Follow the feel of finite state machines even though they won't suffice for this task.
- General process when using the engine: build initial state (players, roles, factions, etc.), query which input type needs to be submitted next, submit input (will output a new immutable state), repeat.
- The previous point also ties into the hierarchy of responsibilities. I don't want the engine to ask for input by calling a provided function or interface because in the case of the assistant app that would mean rendering UI, receving input from there and returning that all in one call (or do crazy async stuff). Either way I'd like to keep UI = fn(state).

## Notes

- This is my first real project with Rust.
- I am much more comfortable with C# than with Rust so I'll prototype the engine in C# before attempting it in Rust. I'd still like to do it in Rust because that basically runs everywhere and is much simpler to embed than C#. Of course I could use MAUI to create the app but frankly I started this whole thing because I wanted to create something useful with flutter.
- I may decide to create a more general engine later on.
- I do have some ideas for the far future like a lua integration to write custom roles etc. but I am trying very hard to ignore these for the time being.

## License

AGPL for now but I might change that at a later stage. If I do switch, I'll make sure that you can still

- create your own characters, rules and adjustments
- create a free api, app, website, tool or anything that uses the library and do whatever with it (publish, keep, ..)
- create a paid/commercial api, app, website, tool or anything that uses the library and do whatever with it (publish, keep, ..)

and I will also always require you to (at the very least)

- publish the changes you make to the library (weakest copyleft I'll probably consider is MPL, up to AGPL)
