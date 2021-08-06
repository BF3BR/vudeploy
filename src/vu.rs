#[path = "lobby.rs"]
mod lobby;

#[path = "server.rs"]
mod server;

pub struct VuDeploy {
    startup_template: String,
    modlist_template: String,
    banlist_template: String,
    lobbies: Vec<lobby::VuLobby>,
    servers: Vec<server::VuServer>
}

impl VuDeploy {
    pub fn new() -> VuDeploy {
        return VuDeploy {
            startup_template: String::new(),
            modlist_template: String::new(),
            banlist_template: String::new(),
            lobbies: Vec::new(),
            servers: Vec::new()
        };
    }

    /// This loads all of the bans
    pub fn load_banlist_template_from_file(&mut self, path: &String) -> bool {
        if path.is_empty() {
            return false;
        }

        match std::fs::read_to_string(path) {
            Ok(template) => self.banlist_template = template.clone(),
            Err(err) => {
                eprintln!("err: could not read banlist template ({}) ({}).", path, err);
                return false;
            }
        }
        return true;
    }

    ///
    pub fn load_banlist_template_from_url(&mut self, url: &String) -> bool {
        let resp = reqwest::blocking::get(url);
        match resp {
            Ok(response) => self.banlist_template = response.text().unwrap_or("".to_string()),
            Err(err) => {
                eprintln!("err: could not download banlist template ({}) ({}).", url, err);
                return false;
            }
        }
        return true;
    }

    /// $SERVERNAME $SERVERPASSWORD
    pub fn load_startup_template_from_file(&mut self, path: &String) -> bool {
        if path.is_empty() {
            return false;
        }

        match std::fs::read_to_string(path) {
            Ok(template) => {
                self.startup_template = template;
            }
            Err(err) => {
                eprintln!("err: ({}).", err);
                return false;
            }
        }

        return true;
    }

    pub fn load_startup_template_from_url(&mut self, url: &String) -> bool {
        if url.is_empty() {
            return false;
        }

        let resp = reqwest::blocking::get(url);
        match resp {
            Ok(response) => self.banlist_template = response.text().unwrap_or("".to_string()),
            Err(err) => {
                eprintln!("err: could not download banlist template ({}) ({}).", url, err);
                return false;
            }
        }
        return true;
    }
}