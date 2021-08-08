#[path = "vu.rs"]
mod vu;

#[path = "lobby.rs"]
mod lobby;

#[path = "server.rs"]
mod server;

#[path = "rcon.rs"]
mod rcon;

use std::{io::{BufRead, BufReader}, process::{Command, Stdio}};

#[test]
fn load_banlist_test() {
    let mut deploy = vu::VuDeploy::new();

    assert!(deploy.load_banlist_template_from_file(&String::from("sample/Admin/BanList.txt")));
}

#[test]
fn lobby_expiration_test() {

}

#[test]
fn experimental_process_test() {
    let is_win32 = cfg!(target_os = "windows");

    // C:\Users\godiwik\AppData\Local\VeniceUnleashed\client\vu.exe -server -dedicated -high60
    let child_process_spawn = Command::new("").stdout(Stdio::piped()).spawn();
    
    match child_process_spawn {
        Ok(mut child_process) => {
            let rip = child_process.stdout.as_mut().unwrap();
            let mut reader = BufReader::new(rip);
            let mut line: String = String::new();
            for i in 0..10 {
                reader.read_line(&mut line);

                println!("line: ({}).", line);
            }
        },
        Err(err) => {
            eprintln!("err: ({}).", err);
            assert!(false);
        }
    }
}

#[test]
fn rcon_connection_test() {
    let rcon = rcon::Rcon::new(&"localhost:47200".to_string());
    
    let stream = &rcon.unwrap().tcp_stream;

    
}