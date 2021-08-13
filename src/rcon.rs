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
use std::ops::IndexMut;

pub struct Rcon {
    pub tcp_stream: TcpStream,
    sequence_number: u32,
}
pub enum SequenceOrigin {
    Server = 0,
    Client = 1
}

pub enum SequenceDirection {
    Request = 0,
    Response = 1
}

pub struct PacketHeader {
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
    /// Packet header size
    ///
    /// sizeof(sequence) + sizeof(size) + sizeof(word_count)
    const PACKET_HEADER_SIZE: usize = 12;

    /// (default: 31)
    pub const PACKET_HEADER_ORIGIN_SHIFT: u32 = 31;

    /// Offset for packet header request/response bit shift
    ///
    /// (default: 30)
    pub const PACKET_HEADER_REQUEST_SHIFT: u32 = 30;
    
    /// (default: 0x3FFFFFFF)
    pub const PACKET_HEADER_SEQUENCE_ID_MASK: u32 = !(3 << PacketHeader::PACKET_HEADER_REQUEST_SHIFT);

    pub fn new_from_reader(reader: &std::io::BufReader<u8>) -> PacketHeader {
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

    pub fn new() -> PacketHeader {
        return PacketHeader {
            sequence: 0,
            size: 0,
            word_count: 0
        }
    }

    pub fn new_from(_sequence: u32, _size: u32, _word_count: u32) -> PacketHeader {
        return PacketHeader {
            sequence: _sequence,
            size: _size,
            word_count: _word_count
        }
    }

    pub fn serialize(&self) -> Vec<u8> {
        let mut data: Vec<u8> = vec![0u8; PacketHeader::PACKET_HEADER_SIZE];

        //let mut cursor= Cursor::new(&data);

        LittleEndian::write_u32(&mut data[0..=3], self.sequence);
        LittleEndian::write_u32(&mut data[4..=7], self.size);
        LittleEndian::write_u32(&mut data[8..=11], self.word_count);

        return data;
    }

    pub fn set_direction(&mut self, direction: SequenceDirection) {
        // set bit
        // number |= 1UL << n;

        // clear bit
        // number &= ~(1UL << n);

        match direction {
            SequenceDirection::Request => {
                self.sequence = self.sequence | (1 << PacketHeader::PACKET_HEADER_REQUEST_SHIFT);
            },
            SequenceDirection::Response => {
                self.sequence = self.sequence & !(1 << PacketHeader::PACKET_HEADER_REQUEST_SHIFT);
            }
        }
    }

    pub fn is_request(&self) -> bool {
        let masked_request_response = self.sequence & (1 << PacketHeader::PACKET_HEADER_REQUEST_SHIFT);
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
        let masked_origin = self.sequence & (1 << PacketHeader::PACKET_HEADER_ORIGIN_SHIFT);

        // If it's 0, then server, 1 is client
        if masked_origin == 0 {
            return SequenceOrigin::Server;
        }
        return SequenceOrigin::Client;
    }

    pub fn set_origin(&mut self, origin: SequenceOrigin) {
        // Get the sequence without the origin bit
        let masked_origin = self.sequence & !(1 << PacketHeader::PACKET_HEADER_ORIGIN_SHIFT);

        match origin {
            SequenceOrigin::Client => {
                // Update teh sequence with a set bit
                self.sequence = masked_origin | (1 << PacketHeader::PACKET_HEADER_ORIGIN_SHIFT);
            },
            SequenceOrigin::Server => {
                // Update the sequence with a cleared bit
                self.sequence = masked_origin;
            }
        }
    }

    pub fn word_count(&self) -> u32 {
        return self.word_count;
    }

    pub fn set_word_count(&mut self, word_count: u32) {
        self.word_count = word_count;
    }

    pub fn size(&self) -> u32 {
        return self.size;
    }

    pub fn set_size(&mut self, size: u32) {
        self.size = size;
    }

    /// Sets the sequence id
    ///
    /// `NOTE: If the sequence id is over the maximum of 0x3FFFFFFF then it resets to 0`
    ///
    pub fn set_sequence_id(&mut self, mut sequence_id: u32) {
        // Bounds check our sequence id
        if sequence_id > PacketHeader::PACKET_HEADER_SEQUENCE_ID_MASK {
            sequence_id = 0;
        }

        // Clear the sequence number, preserving the upper 2 bits
        let cleared_sequence = self.sequence & !PacketHeader::PACKET_HEADER_SEQUENCE_ID_MASK;

        // Update the sequence with the new sequence id
        self.sequence = cleared_sequence | sequence_id;
    }

    /// Returns the combined sequence
    ///
    /// This will contain the sequence id, the origin, and the request/response combined
    pub fn sequence(&self) -> u32 {
        return self.sequence
    }

    /// Returns the sequence id
    ///
    pub fn sequence_id(&self) -> u32 {
        return self.sequence & PacketHeader::PACKET_HEADER_SEQUENCE_ID_MASK;
    }
}

pub struct RemotePacket {
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