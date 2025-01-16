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
            byte[] msgbuffer;
            var length = ReadInt32();
            msgbuffer = new byte[length];
            _stream.Read(msgbuffer, 0, length);

            var msg = Encoding.UTF8.GetString(msgbuffer);
            return msg;

        }
    }
}
