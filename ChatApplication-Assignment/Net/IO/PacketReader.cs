using System.IO;
using System.Text;


namespace ChatClient.Net.IO
{
    public class PacketReader : BinaryReader
    {
        private Stream _stream;
        public PacketReader(Stream stream) : base(stream)
        {
            _stream = stream;
        }

        public async Task<byte> ReadOpcodeAsync()
        {
            var buffer = new byte[1];
            int read = await BaseStream.ReadAsync(buffer, 0, 1);
            if (read != 1)
                throw new IOException("Failed to read opcode: connection closed or incomplete data.");
            return buffer[0];
        }

        public async Task<string> ReadStringAsync()
        {
            var lengthBuffer = new byte[4];
            int read = await BaseStream.ReadAsync(lengthBuffer, 0, 4);
            if (read != 4)
                throw new IOException("Failed to read string length: connection closed or incomplete data.");

            var length = BitConverter.ToInt32(lengthBuffer, 0);
            if (length < 0 || length > 10000)
                throw new IOException($"Invalid string length: {length}");

            var stringBuffer = new byte[length];
            read = await BaseStream.ReadAsync(stringBuffer, 0, length);
            if (read != length)
                throw new IOException("Failed to read full string data.");

            return Encoding.UTF8.GetString(stringBuffer);
        }
    }
}
