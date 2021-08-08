use std::net::TcpStream;
use packed_struct::prelude::*;

pub struct Rcon {
    pub tcp_stream: TcpStream,
    sequence_number: u32,
}

struct Sequence {
    origin: SequenceOrigin,
    direction: SequenceDirection,
    sequence_id: u32
}
enum SequenceOrigin {
    Server = 0,
    Client = 1
}

enum SequenceDirection {
    Request = 0,
    Response = 1
}

struct PacketHeader {
    /// Bit 31
    /// 
    /// - 0 The command in this command/response pair originated on the server
    /// - 1 The command in this command/response pair originated on the client
    ///
    /// Bit 30
    /// - 0 Request
    /// - 1 Response
    ///
    /// Bits 29..0 Sequence Number
    sequence: u32,

    /// Total size of packet, in bytes
    size: u32,
    word_count: u32
}

struct PacketWord {
    size: u32,
    content: String
}

impl Rcon {
    pub fn new(connection_address: &String) -> Option<Rcon> {
        let connection_stream_result = TcpStream::connect(connection_address);
        match connection_stream_result {
            Ok(connection_stream) => {
                return Some(Rcon {
                    tcp_stream: connection_stream,
                    sequence_number: 0
                });
            },
            Err(err) => {
                eprintln!("err: could not connect ({}).", err);
                return None;
            }
        }
    }
}