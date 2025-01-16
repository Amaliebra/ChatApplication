using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net.IO
{
    public class PacketBuilder
    {
        private MemoryStream _packetStream;

        public PacketBuilder()
        {
            _packetStream = new MemoryStream();
        }

        public void WriteOpCode(byte opCode)
        {
            _packetStream.WriteByte(opCode);
        }

        public void WriteString(string message)
        {
            var length = Encoding.UTF8.GetByteCount(message);
            _packetStream.Write(BitConverter.GetBytes(length), 0, 4);
            _packetStream.Write(Encoding.UTF8.GetBytes(message), 0, length);
        }

        public byte[] GetPacketBytes()
        {
            return _packetStream.ToArray();
        }

        public void Reset()
        {
            _packetStream = new MemoryStream();
        }
    }
}