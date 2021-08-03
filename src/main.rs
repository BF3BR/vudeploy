use hyper::{Body, Request, Response, Server};
// Import the routerify prelude traits.
use routerify::prelude::*;
use routerify::{Router, RouterService};
use std::io;
use std::net::SocketAddr;
use uuid::Uuid;

/*
Web API
Create New Server
Reconfigure Running Server
Get Server Logs/Crashed

*/

struct VuDeploy {
    startup_template: String,
    modlist_template: String,
    banlist_template: String,
}

impl VuDeploy {
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
}

struct VuServer {
    id: Uuid,
    prefix_override: String,
    user_password: String,


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


fn create_server_config(serverName: String, serverPass: String, key: String) -> String {
    return String::from(format!(""));
}


// A handler for "/" page.
async fn home_handler(_: Request<Body>) -> Result<Response<Body>, io::Error> {
    Ok(Response::new(Body::from("Home page")))
}

// Define a handler for "/users/:userName/books/:bookName" page which will have two
// route parameters: `userName` and `bookName`.
async fn user_book_handler(req: Request<Body>) -> Result<Response<Body>, io::Error> {
    let user_name = req.param("userName").unwrap();
    let book_name = req.param("bookName").unwrap();

    Ok(Response::new(Body::from(format!(
        "User: {}, Book: {}",
        user_name, book_name
    ))))
}

async fn create_new_server(_req: Request<Body>) -> Result<Response<Body>, io::Error> {
    let user_name = "guid";
    let book_name = "rip";
    Ok(Response::new(Body::from(format!(
        "User: {}, Book: {}",
        user_name, book_name
    ))))
}

fn router() -> Router<Body, io::Error> {
    // Create a router and specify the the handlers.
    Router::builder()
        .get("/", home_handler)
        .get("/users/:userName/books/:bookName", user_book_handler)
        .build()
        .unwrap()
}

#[tokio::main]
async fn main() {
    let router = router();

    // Create a Service from the router above to handle incoming requests.
    let service = RouterService::new(router).unwrap();

    // The address on which the server will be listening.
    let addr = SocketAddr::from(([127, 0, 0, 1], 3001));

    // Create a server by passing the created service to `.serve` method.
    let server = Server::bind(&addr).serve(service);

    println!("App is running on: {}", addr);
    if let Err(err) = server.await {
        eprintln!("Server error: {}", err);
    }
}