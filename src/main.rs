fn main() {
}

struct Werewolf {}
impl Role for Werewolf {
    fn get_night_action(&self, game: &Game) -> Option<Box<dyn PendingAction>> {
        WerewolfNightAction::default()
    }
}

#[derive(Default)]
struct WerewolfNightAction {}
impl PendingAction for WerewolfNightAction {
    fn get_input_schema(&self, game: &Game) -> Box<dyn InputSchema> {
        Box::new(WerewolfNightActionSchema::default())
    }

    fn submit_input(&self, input: Box<dyn InputSubmission>) -> Box<dyn ReadyAction> {
        Box::new(WerewolfNightActionReady::new(input))
    }
}

struct WerewolfNightActionReady {}
impl ReadyAction for WerewolfNightActionReady {
    fn transform<'a, 'b, 'c>(&'a self, game: Game<'b>) -> Game<'c> {
        
    }
}

#[derive(Default)]
struct WerewolfNightActionSchema {}
impl InputSchema for WerewolfNightActionSchema {}

struct WerewolfNightActionSubmission<'a> {
    target: Option<&'a Player<'a>>
}
impl<'a> WerewolfNightActionSubmission<'a> {
    pub fn new(target: Option<&Player>) -> Self {
        WerewolfNightActionSubmission {
            target
        }
    }
}
impl<'a> InputSubmission for WerewolfNightActionSubmission<'a> {}

struct Game<'a> {
    players: Vec<Player<'a>>,
    pending_action: Box<dyn PendingAction>,
}

impl<'a> Game<'a> {
    pub fn submit_input(self, input: Box<dyn InputSubmission>) -> Self {
        let ready_action = self.pending_action.submit_input(input);

        let mut game = ready_action.transform(self);
        game.pending_action = game.get_next_action();

        game
    }

    pub fn get_input_schema(&self) -> Box<dyn InputSchema> {
        self.pending_action.get_input_schema(self)
    }

    fn get_next_action(&self) -> Box<dyn PendingAction> {
        unimplemented!()
    }
}

struct Player<'a> {
    roles: Vec<&'a dyn Role>,
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
trait InputSubmission {}
