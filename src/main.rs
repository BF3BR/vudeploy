use hyper::{Body, Request, Response, Server, StatusCode};
use routerify::{RequestInfo, prelude::*};
use routerify::{Router, RouterService};
use std::io;
use std::net::SocketAddr;
use uuid::Uuid;

// Testing module
//#[cfg(tests)]
mod tests;

mod lobby;
mod server;
mod vu;

/*
Web API
Create New Server
Reconfigure Running Server
Get Server Logs/Crashed

*/

///
fn create_server_config(server_name: String, server_pass: String, key: String) -> String {
    return String::from(format!(""));
}


// A handler for "/" page.
async fn home_handler(_: Request<Body>) -> Result<Response<Body>, io::Error> {
    Ok(Response::new(Body::from("")))
}

async fn error_handler(err: routerify::RouteError, _: RequestInfo) -> Response<Body> {
    eprintln!("{}", err);
    Response::builder()
        .status(StatusCode::INTERNAL_SERVER_ERROR)
        .body(Body::from(format!("Something went wrong: {}", err)))
        .unwrap()
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

async fn create_new_lobby(_req: Request<Body>) -> Result<Response<Body>, io::Error> {
    
    return Ok(Response::new(Body::from(format!(""))));
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