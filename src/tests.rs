#[path = "vu.rs"]
mod vu;

#[path = "lobby.rs"]
mod lobby;

#[path = "server.rs"]
mod server;

#[path = "rcon.rs"]
mod rcon;

use std::{io::{BufRead, BufReader}, process::{Command, Stdio}};
use byteorder::LittleEndian;
use futures::future::Future;
use byteorder::ByteOrder;

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
    let child_process_spawn = Command::new("C:\\Users\\godiwik\\AppData\\Local\\VeniceUnleashed\\client\\vu.exe").arg("-server").arg("-dedicated").arg("-high60)").arg("-headless").stdout(Stdio::piped()).spawn();
    
    match child_process_spawn {
        Ok(mut child_process) => {
            let rip = child_process.stdout.as_mut().unwrap();
            let mut reader = BufReader::new(rip);
            
            let _read_log = async {
                loop {
                    let mut line: String = String::new();
                    let read_result = reader.read_line(&mut line);
                    match read_result {
                        Ok(bytes_read) => {
                            if bytes_read == 0 {
                                break;
                            }
                            //println!("line: ({}).", line);

                        },
                        Err(err) => {
                            eprintln!("could not read from stdout ({}).", err);
                            break;
                        }
                    }
                }
            };
        },
        Err(err) => {
            eprintln!("err: ({}).", err);
            assert!(false);
        }
    }

    loop {
        let _sig = b"Sig";
    }
}

#[test]
fn rcon_connection_test() {
    let rcon = rcon::Rcon::new(&"localhost:47200".to_string());
    
    let stream = &rcon.unwrap().tcp_stream;

    
}

#[test]
fn rcon_serialization_test() {
    // Create a new packet header
    let packet_header = rcon::PacketHeader::new_from(123, 456, 789);

    // Serialize the data
    let data = &packet_header.serialize();

    // Deserialize from the buffer and verify results
    assert_eq!(LittleEndian::read_u32(&data[0..=3]), packet_header.sequence());
    assert_eq!(LittleEndian::read_u32(&data[4..=7]), packet_header.size());
    assert_eq!(LittleEndian::read_u32(&data[8..=11]), packet_header.word_count());
}

#[test]
fn rcon_bit_tests() {
    let mut packet_header = rcon::PacketHeader::new();

    &packet_header.set_sequence_id(123);
    &packet_header.set_origin(rcon::SequenceOrigin::Client);
    
}

#[test]
fn rcon_const_tests() {
    assert_eq!(rcon::PacketHeader::PACKET_HEADER_REQUEST_SHIFT, 30);
    assert_eq!(rcon::PacketHeader::PACKET_HEADER_ORIGIN_SHIFT, 31);
    assert_eq!(rcon::PacketHeader::PACKET_HEADER_SEQUENCE_ID_MASK, 0x3FFFFFFF);
    assert_eq!(!rcon::PacketHeader::PACKET_HEADER_SEQUENCE_ID_MASK, 0xC0000000);
}