# Concept

- Rule evaluation with tags and master tags is already pretty well-thought-out and implemented partially in the C# project.
- The engine should only be responsible for advancing game state according to input. UI is therefore
  not part of this engine and must be some external function that takes the game state (`UI = fn(state)`). The game can be advanced by giving it the correct input, which, in foreseeable applications, would be taken from user input on the UI, or some artificial intelligence function.
- Game states should be self-contained to allow a history of game states which shows
  the progression of the game and avoid bugs together with strict immutability practices. This would also allow trees where you can branch on a certain state with
  a different action.

## Game states

Arguably the most important design decision is how the game state is represented. How one state
is transformed into another follows from that together with the tagging concept and is in my maybe naive mind, simpler.

### What does a game state need to capture

- Players
  - Name/identifier
  - Living or dead
  - Tags (night tags and persistent tags?)
    - Tags must also store by whom the tag was added and which action caused the tag to be added. At least it would be a very big help for debugging and explainability of the whole thing but some actions may even need it. You could argue however, that tags only change when some action happens, so analyzing the changes from before and after an action could suffice, and also tons of logs in the beginning, that a lot easier than doing meta data from the beginning.
  - Roles
  - Factions (this one I'm not sure about how to use/derive, urwolf, amor and mafia come to mind)
- List/queue of actions until next action gathering
  - I imagine that one "action gathering" queries all roles for actions that need to happen
    according to their implementation and the current game state. These are then listed in a queue.
- List of past actions since the last action gathering maybe?
- Round count
- Phase (day/night). Maybe this fits nicely into the actions, that every other action gathering just
  puts in the day actions because some roles also have day actions e.g. Henker or Stotternder Ritter. The core game loop could execute actions when there are remaining ones and if there are none, collapse tags, flip the day/night, do an action gathering and repeat.

For developing this structure, I try something, then ask myself if it's obvious and unambiguous what the next step is.

E.g. initial state could be

[Example a: initial state](./example-game/a00_inital-state.json)

The state is self-contained but the here displayed JSON representation obviously doesn't contain the
implementation of the roles, factions and actions. Roles definitely _can_ have data attached, like the
number of remaining spells for the witch, while factions being represented as just their ID is probably fine.
Currently, I cannot think of an action that has data associated with it since they are generally just
functions that transform a state into another based on the current tags, order of players, etc. (the game state).

In this state, the game can be queried for the current action which contains at least the type of input that
the game needs to be able to advance.

The outside agent, that forwards user input or makes decisions itself, uses all the information from the
current action and does whatever it needs to obtain input data of the correct type. This input data
can then be fed to the game which either accepts or rejects it. If it accepts it, it will return a new
game state in which the action was executed with the provided input data.

For our example, the GM app would take the `werewolf_vote` action (which probably doesn't need to contain any
data itself) and query the GM which player was selected to be killed (or none if not unanimous). The GM
selects the player and the GM app puts that information into a data structure of the type that the action
is requesting then passes that to the game object. If successful, the current game state in the GM app is updated
to something like this.

Here the werewolf has voted that `P1-villager` should be killed.

[Example a: werewolves selected P1](./example-game/a01_werewolves-selected-P1.json)

Now the GM app would take the current action `witch_heal-or-kill` and query the GM for what
the witch wants to do. All the information needed can be found in the game state, namely the tags
where the "killed_by_werewolves" tag can be found. It might make sense to have a helper function
on the action that fetches the name of the werewolf-target from the tags so that the GM app doesn't
have to that on its own. The GM app would then present the three options, heal, kill or do nothing
and gather the input of the GM in a response struct.

Suppose the witch chose to kill the remaining person "P3-werewolf", and the response containing
that decision is passed to the game, the game state could be updated as follows.

[Example a: witch killed werewolf](./example-game/a02_witch-killed-werewolf.json)

Note that there are no more remaining actions, which is the sign that the tags
must be collapsed. This is done using the tag rules to collapse each tag set to
a set of master tags, which boil down to "killed" in this case.

[Example a: tags collapsed](./example-game/a03_tags-collapsed.json)

Once all tags are collapsed, consequences follow. Note that when consequences are executed, the
tags are removed again.

[Example a: tag consequences](./example-game/a04_tag-consequences.json)

Then all the factions that are in play can be queried to see if they have won.
In this case, the village faction will return that the village has won because
all the werewolves were killed. At this point the game has ended, and no new actions will be enqueued.

[Example a: game ended](./example-game/a05_game-ended.json)

I think I'll try modeling this now before diving deeper for the needs of other roles etc.

TODO it should probably only be one faction. Whenever a player's roles change, the highest priority
faction of all the role factions should be set as the active faction of the player.

TODO for in-love, implement a life link core game mechanic.

