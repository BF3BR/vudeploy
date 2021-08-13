use std::io::Cursor;
use std::net::TcpStream;
use futures::StreamExt;
use hyper::body::Buf;
use tokio::io;
use tokio_util::codec::{BytesCodec, FramedRead, FramedWrite};

use std::env;
use std::error::Error;
use std::net::SocketAddr;
use byteorder::{ByteOrder, LittleEndian};

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

impl PacketHeader {
    pub fn new(reader: &std::io::BufReader<u8>) -> PacketHeader {
        let mut cursor = std::io::Cursor::new(reader.buffer());
        let sequence_ = cursor.get_u32_le();
        let size_ = cursor.get_u32_le();
        let word_count_ = cursor.get_u32_le();

        return PacketHeader {
            sequence: sequence_,
            size: size_,
            word_count: word_count_
        };
    }

    pub fn serialize(&self) -> Vec<u8> {
        let mut data: Vec<u8> = Vec::new();

        return data;
    }

    pub fn is_request(&self) -> bool {
        let masked_request_response = self.sequence & 0x40000000;
        if masked_request_response == 0 {
            return true;
        }
        return false;
    }

    pub fn is_response(&self) -> bool {
        return !self.is_request();
    }

    pub fn origin(&self) -> SequenceOrigin {
        // We only want the 31'st bit
        let masked_origin = self.sequence & 0x80000000;

        // If it's 0, then server, 1 is client
        if masked_origin == 0 {
            return SequenceOrigin::Server;
        }
        return SequenceOrigin::Client;
    }

    pub fn word_count(&self) -> u32 {
        return self.word_count;
    }

    pub fn size(&self) -> u32 {
        return self.size;
    }
}

struct RemotePacket {
    header: PacketHeader,
    words: Vec<String>,
}

impl RemotePacket {
    pub fn new() -> RemotePacket {
        return RemotePacket {
            header: PacketHeader { 
                sequence: 0, 
                size: 0, 
                word_count: 0 
            },
            words: Vec::new()
        }
    }

    fn serialize_words(&mut self) -> Vec<u8> {
        let mut string_data: Vec<u8> = Vec::new();

        for word in &self.words {
            // Verify that the word is not empty
            if word.is_empty() {
                continue;
            }

            // I don't know what the hell this does, but hey whatever
            if !word.is_ascii() {
                continue;
            }
            
            // Get the word length and serialize it to byte array
            let word_length = word.len();
            let mut word_data = vec![0u8; 4];
            LittleEndian::write_u32(word_data.as_mut_slice(), word_length as u32);

            // Add the size of the data as a byte array
            string_data.extend(&word_data);

            // Add the string data
            string_data.extend(word.as_bytes());

            // Add a null terminator
            string_data.extend(b"\0");

            // Increment each of our word counts
            self.header.word_count += 1;
        }

        return string_data;
    }

    pub fn serialize(&mut self, mut cursor: &std::io::Cursor<u8>) -> Vec<u8> {
        let mut final_data: Vec<u8> = Vec::new();

        let serialized_words_data = self.serialize_words();

        let word_count = self.header.word_count;
        let word_data_size = serialized_words_data.len() as u32;
        //final_data.write_u32(self.)

        return final_data;
    }

    pub fn read_from_reader(_cursor: &std::io::Cursor<u8>) {
        
    }
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

    pub fn read() {

    }
}