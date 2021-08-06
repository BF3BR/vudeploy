#[path = "vu.rs"]
mod vu;

#[path = "lobby.rs"]
mod lobby;

#[path = "server.rs"]
mod server;

#[test]
fn load_banlist_test() {
    let mut deploy = vu::VuDeploy::new();

    assert!(deploy.load_banlist_template_from_file(&String::from("sample/Admin/BanList.txt")));
}