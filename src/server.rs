

/// Representation of a vu server
/// 
/// This will also need to be tied to an actual running process with the ability to catch/handle if it crashes/disappears
/// This will also need to be able to gather the file logs, stdout output, and the frostbite logs and be able to pipe them over a website at any point
/// Having the ability to gracefully shut down and start up servers is also a bonus
pub struct VuServer {
    /// The id of this server
    id: uuid::Uuid,

    /// The prefix that is added to the server name
    prefix_override: String,

    /// The password to the VU server for users
    user_password: String,

    /// Finalized ModList.txt
    mod_list: String,

    /// Finalized MapList.txt
    map_list: String,

    /// Finalized BanList.txt
    ban_list: String,

    /// Finalized Startup.txt
    startup: String
}

impl VuServer {
    /*pub fn new() -> VuServer {
        return VuServer {
            id: Uuid::new_v4(),
            prefix_override: String::new(),
            user_password: Uuid::new_v4().to_string()
        }
    }*/

    /// get_prefix
    /// 
    /// Returns the prefix that all servers will use
    pub fn get_prefix(&self) -> String {
        if self.prefix_override.is_empty() {
        return String::from("vu_");
        }
        return self.prefix_override.clone();
    }

    pub fn get_id_as_string(&self) -> String {
        return String::from(self.id.to_string());
    }

    pub fn get_config(&self) -> String {
        return String::new();
    }

    pub fn create_new_br_startup(&self) -> String {
        return String::from(format!("vars.serverName \"{}\" \r\nvars.friendlyFire true\r\nadmin.password \"cows\"", self.get_display_name()));
    }

    pub fn get_display_name(&self) -> String {
        return String::from(format!("{}{}", self.get_prefix(), self.get_id_as_string()))
    }

    pub fn get_user_password(&self) -> String {
        return self.user_password.clone();
    }
}