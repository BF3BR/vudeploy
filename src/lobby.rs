
pub enum VuLobbyStatus {
    Waiting,
    InGame,
}

///
/// The lobby implementation
/// 
/// This is responsible for keeping all players in a lobby, searching that entire lobby to be inserted into a game as well as
pub struct VuLobby {
    id: uuid::Uuid,
    status: VuLobbyStatus,
    players: Vec<uuid::Uuid>,
    max_players: usize,
}

impl VuLobby {
    const DEFAULT_MAX_PLAYERS: usize = 4;

    pub fn new() -> VuLobby {
        return VuLobby {
            id: uuid::Uuid::new_v4(),
            status: VuLobbyStatus::Waiting,
            players: Vec::new(),
            max_players: VuLobby::DEFAULT_MAX_PLAYERS
        }
    }

    /// Adds a player to the lobby
    pub fn add_player(&mut self, player_id: uuid::Uuid) -> bool {
        // Verify that there is space in this lobby
        if self.players.len() >= self.max_players {
            eprintln!("err: lobby already reached max players.");
            return false;
        }

        // Check if we already have this player, if so then return success but do not add the duplicate
        if self.players.contains(&player_id) {
            eprintln!("warn: dupe player id ({}) attempting to be added to lobby ({}).", player_id, self.id);
            return true;
        }

        // Add the player id to this lobby
        self.players.push(player_id);

        return true;
    }

    /// Removes a player from the lobby
    pub fn remove_player(&mut self, player_id: uuid::Uuid) -> bool {
        if self.players.contains(&player_id) {
            // xs.retain(|&x| x != some_x);
            self.players.retain(|&x| x != player_id);
            return true;
        }

        eprintln!("err: player ({}) not found in lobby for removal.", &player_id);
        return false;
    }
}