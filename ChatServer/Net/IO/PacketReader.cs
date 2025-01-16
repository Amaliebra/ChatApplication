using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Net.IO
{
    public class PacketReader : BinaryReader
    {
        private Stream _stream;
        public PacketReader(Stream stream) : base(stream)
        {
            _stream = stream;
        }

        public byte ReadOpcode()
        {
            return ReadByte();
        }

        public string ReadString()
        {
            var length = ReadUInt16();
            var message = new string(ReadChars(length));
            return message;
        }

        public async Task<byte> ReadOpcodeAsync()
        {
            var buffer = new byte[1];
            await BaseStream.ReadAsync(buffer, 0, 1);
            return buffer[0];
        }

        public async Task<string> ReadStringAsync()
        {
            var lengthBuffer = new byte[2];
            await BaseStream.ReadAsync(lengthBuffer, 0, 2);
            var length = BitConverter.ToInt16(lengthBuffer, 0);

            var stringBuffer = new byte[length];
            await BaseStream.ReadAsync(stringBuffer, 0, length);
            return Encoding.UTF8.GetString(stringBuffer);

        }
    }
}
