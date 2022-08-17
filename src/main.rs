/* TODOs
 * - try to use generics for example the night actions might benefit from them but you
 *   need non-generic traits as well otherwise you'll be in Any hell which you'd love to avoid
 * - take less ownership of things. Maybe moving is nice for performance and consuming is nice for
 *   semantics but in the end it just makes things more complicated and since you're calling from
 *   dart which owns all the variables anyway, it may even cause issues.
 */

fn main() {
    let all_roles = vec![Werewolf::default()];
    let mut history = vec![];
    let mut game = Game {
        players: vec![Player {
            name: "Joel".to_owned(),
            roles: vec![&all_roles[0]],
        }],
        pending_action: None,
    };

    loop {
        history.push(game);
        let input_schema = game.get_input_schema();
        let input = request_input(input_schema.as_ref());
        game = game.submit_input(input);
    }
}

fn request_input(input_schema: &dyn InputSchema) -> Box<dyn InputSubmission<'static>> {
    unimplemented!();
}

#[derive(Default)]
struct Werewolf {}
impl Role for Werewolf {
    fn get_night_action(&self, _game: &Game) -> Option<Box<dyn PendingAction>> {
        Some(Box::new(WerewolfNightAction::default()))
    }
}

#[derive(Default)]
struct WerewolfNightAction {}
impl PendingAction for WerewolfNightAction {
    fn get_input_schema(&self, _game: &Game) -> Box<dyn InputSchema> {
        Box::new(WerewolfNightActionSchema::default())
    }

    fn submit_input<'a>(&self, input: Box<dyn InputSubmission + 'a>) -> Box<dyn ReadyAction + 'a> {
        input.prepare_action()
    }
}

struct WerewolfNightActionReady<'a> {
    input: Box<WerewolfNightActionSubmission<'a>>,
}
impl<'d> ReadyAction for WerewolfNightActionReady<'d> {
    fn transform<'a, 'b, 'c>(&'a self, game: Game<'b>) -> Game<'c>
    where
        'b: 'c,
    {
        let mut new_players = game.players.clone();
        if let Some(killed_player) = self.input.target {
            let killed_player_index = game
                .players
                .iter()
                .position(|p| p == killed_player)
                .expect("killed Player to be one of the Players");
            new_players.remove(killed_player_index);
        }

        Game {
            players: new_players,
            pending_action: None,
        }
    }
}

#[derive(Default)]
struct WerewolfNightActionSchema {}
impl InputSchema for WerewolfNightActionSchema {}

struct WerewolfNightActionSubmission<'a> {
    target: Option<&'a Player<'a>>,
}
impl<'a> WerewolfNightActionSubmission<'a> {
    pub fn new(target: Option<&'a Player>) -> Self {
        WerewolfNightActionSubmission { target }
    }
}
impl<'a> InputSubmission<'a> for WerewolfNightActionSubmission<'a> {
    fn prepare_action(self: Box<WerewolfNightActionSubmission<'a>>) -> Box<dyn ReadyAction + 'a> {
        Box::new(WerewolfNightActionReady::<'a> { input: self })
    }
}

struct Game<'a> {
    players: Vec<Player<'a>>,
    pending_action: Option<Box<dyn PendingAction>>,
}

impl<'a> Game<'a> {
    pub fn initialize(players: Vec<Player>) -> Game {
        let mut game = Game {
            players,
            pending_action: None,
        };

        game.pending_action = game.get_next_action();

        game
    }

    pub fn submit_input(self, input: Box<dyn InputSubmission>) -> Self {
        let ready_action = self.get_pending_action().submit_input(input);

        let mut game = ready_action.transform(self);
        game.pending_action = game.get_next_action();

        game
    }

    pub fn get_input_schema(&self) -> Box<dyn InputSchema> {
        self.get_pending_action().get_input_schema(self)
    }

    fn get_next_action(&self) -> Option<Box<dyn PendingAction>> {
        unimplemented!()
    }

    fn get_pending_action(&self) -> &Box<dyn PendingAction> {
        if let Some(pending_action) = &self.pending_action {
            pending_action
        } else {
            panic!("No pending action. TODO this should probably be a result instead.")
        }
    }
}

#[derive(Clone)]
struct Player<'a> {
    name: String, // unique
    roles: Vec<&'a dyn Role>,
}

impl<'a> PartialEq for Player<'a> {
    fn eq(&self, other: &Self) -> bool {
        self.name.eq(&other.name)
    }
}

trait Role {
    // TODO the role should know where to put the night_action in the execution order for the night
    fn get_night_action(&self, _game: &Game) -> Option<Box<dyn PendingAction>> {
        unimplemented!();
    }
}

trait PendingAction {
    fn get_input_schema(&self, game: &Game) -> Box<dyn InputSchema>;
    fn submit_input(&self, input: Box<dyn InputSubmission>) -> Box<dyn ReadyAction>;
}

trait ReadyAction {
    // it took a while but you have to tell the compiler explicitly that only the game parameter
    // has to outlive the return value so that 1 it doesn't think the return value is connected to
    // self and 2 probably something about moving otherwise I don't understand why it's necessary
    // given that the method consumes the game parameter anyway. I'm still a rust noob after all.
    fn transform<'a, 'b, 'c>(&'a self, game: Game<'b>) -> Game<'c>
    where
        'b: 'c;
}

trait InputSchema {}
trait InputSubmission<'a> {
    fn prepare_action(self: Box<Self>) -> Box<dyn ReadyAction + 'a>;
}
