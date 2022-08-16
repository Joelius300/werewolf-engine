# Werewolf engine

This is an unfinished attempt at a werewolf engine according to [these rules](https://joelius300.github.io/werewolf-guide/) built for a very specific purpose that is a GM assistant app. This GM assistant app will be built afterwards using [flutter_rust_bridge](https://github.com/fzyzcjy/flutter_rust_bridge) and has the following tasks:

- keep track of / alert at edge cases
- keep history of events

## Design

In order to achieve the goals I set myself for the assistant app, I will be trying to adhere to these design guidelines:

- All state is immutable
- Follow the feel of finite state machines even though they won't suffice for this task
- General process: build state (players, roles, factions, etc.), query which input type needs to be submitted next, submit input which will output a new immutable state

## Notes

- This is my first real project with Rust.
- I may decide to create a more general engine later on.
