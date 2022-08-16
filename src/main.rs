fn main() {}

struct Werewolf {}
impl Role for Werewolf {
    fn get_night_action(&self, game: &Game) -> Option<Box<dyn PendingAction>> {
        Some(Box::new(WerewolfNightAction::default()))
    }
}

#[derive(Default)]
struct WerewolfNightAction {}
impl PendingAction for WerewolfNightAction {
    fn get_input_schema(&self, game: &Game) -> Box<dyn InputSchema> {
        Box::new(WerewolfNightActionSchema::default())
    }

    fn submit_input(&self, input: Box<dyn InputSubmission>) -> Box<dyn ReadyAction> {
        input.prepare_action()
    }
}

struct WerewolfNightActionReady<'a> {
    input: &'a WerewolfNightActionSubmission<'a>,
}
impl<'d> ReadyAction for WerewolfNightActionReady<'d> {
    fn transform<'a, 'b, 'c>(&'a self, game: Game<'b>) -> Game<'c> {
        let mut new_players = game.players.clone();
        if let Some(killed_player) = self.input.target {
            let killed_player_index = game
                .players
                .iter()
                .position(|p| p == killed_player)
                .expect("killed Player to be one of the Players");
            new_players.remove(killed_player_index);
        }

        Game::<'c> {
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
impl<'a> InputSubmission for WerewolfNightActionSubmission<'a> {
    fn prepare_action(&self) -> Box<dyn ReadyAction> {
        Box::new(WerewolfNightActionReady { input: self })
    }
}

struct Game<'a> {
    players: Vec<Player<'a>>,
    pending_action: Option<Box<dyn PendingAction>>,
}

impl<'a> Game<'a> {
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
    fn get_night_action(&self, game: &Game) -> Option<Box<dyn PendingAction>> {
        unimplemented!();
    }
}

trait PendingAction {
    fn get_input_schema(&self, game: &Game) -> Box<dyn InputSchema>;
    fn submit_input(&self, input: Box<dyn InputSubmission>) -> Box<dyn ReadyAction>;
}

trait ReadyAction {
    // it took a while but you have to tell the compiler explicitly that none of these arguments
    // or return types are connected, otherwise it'll think the returned game is connected to self.
    fn transform<'a, 'b, 'c>(&'a self, game: Game<'b>) -> Game<'c>;
}

trait InputSchema {}
trait InputSubmission {
    fn prepare_action(&self) -> Box<dyn ReadyAction>;
}
