
use chrono::{DateTime, Duration, Utc};

/// Current lobby status
/// 
/// `Waiting` - Players are currently waiting and the lobby is joinable
/// 
/// `InGame` - Players are in-game and lobby is not joinable
/// 
/// The way that these are supposed to be used is, if InGame players who have already
/// joined the lobby are able to join back
pub enum VuLobbyStatus {
    /// Lobby is waiting and accepting new players
    Waiting,

    /// Lobby is in-game and not accepting new players
    InGame,
}

///
/// The lobby implementation
/// 
/// This is responsible for keeping all players in a lobby, searching that entire lobby to be inserted into a game as well as
pub struct VuLobby {
    /// Current lobby id
    id: uuid::Uuid,

    /// Current lobby status
    status: VuLobbyStatus,

    /// Current id of players joined to the lobby
    players: Vec<uuid::Uuid>,

    /// Maximum amount of players allowed in the lobby
    max_players: usize,

    /// Creation time of this lobby
    creation_time: DateTime<Utc>,

    /// Expiry time of this lobby
    expiry_time: DateTime<Utc>
}

impl VuLobby {
    /// Default maximum player count (default: 4)
    const DEFAULT_MAX_PLAYERS: usize = 4;

    /// Default expiry time in MINUTES
    const DEFAULT_EXPIRY_TIME: usize = 5;

    /// Creates a new instance of a VuLobby in waiting state
    /// 
    /// Uses the `VuLobby::DEFAULT_MAX_PLAYERS` for player count
    pub fn new() -> VuLobby {
        // Get a new creation time so we can calculate the expiry time
        let creation_time_ = Utc::now();

        // Return our new instance
        return VuLobby {
            id: uuid::Uuid::new_v4(),
            status: VuLobbyStatus::Waiting,
            players: Vec::new(),
            max_players: VuLobby::DEFAULT_MAX_PLAYERS,
            creation_time: creation_time_,
            expiry_time: creation_time_.checked_add_signed(Duration::minutes(VuLobby::DEFAULT_EXPIRY_TIME as i64)).unwrap()
        }
    }

    /// Adds a player to the lobby
    /// 
    /// `player_id` - Player uuid
    /// 
    /// Returns `true` on success, `false` otherwise
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
    /// 
    /// `player_id` - Player uuid
    /// 
    /// Returns `true` on success, `false` otherwise
    pub fn remove_player(&mut self, player_id: uuid::Uuid) -> bool {
        // Check to see if the player id is contained in our list
        if self.players.contains(&player_id) {
            
            // If so create a new list retaining all guids that are NOT our player id
            self.players.retain(|&x| x != player_id);

            return true;
        }

        // We did not find the player for removal.
        eprintln!("err: player ({}) not found in lobby for removal.", &player_id);
        return false;
    }

    /// Extends the expiration time of this lobby by the default expiry time
    /// 
    /// This will return `false` if the current lobby has already expired
    /// 
    /// Returns `true` on success, `false` otherwise
    pub fn extend_expiry(&mut self) -> bool {
        let current_time_ = Utc::now();

        // Check to see if the lobby has already expired
        if current_time_ >= self.expiry_time {
            eprintln!("err: attempted to extend already expired lobby.");
            return false;
        }

        // Calculate the new expiry time
        let extended_time_ = current_time_.checked_add_signed(Duration::minutes(VuLobby::DEFAULT_EXPIRY_TIME as i64)).unwrap();
        
        // Set the new expiry time
        self.expiry_time = extended_time_;

        return true;
    }

    /// Returns if this lobby is expired and should be removed
    /// 
    pub fn is_expired(&self) -> bool {
        return Utc::now() >= self.expiry_time;
    }
}