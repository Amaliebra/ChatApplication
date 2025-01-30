
using ChatServer.Net.IO;
using System.Net;
using System.Net.Sockets;

public class Program
{
    private TcpListener _server;
    private List<TcpClient> _clients;

    public Program(string ip, int port)
    {
        _server = new TcpListener(IPAddress.Any, port);
        _clients = new List<TcpClient>();
    }

    public static async Task Main(string[] args)
    {
        var program = new Program("127.0.0.1", 5000);
        await program.StartAsync();
    }

    public async Task StartAsync()
    {
        _server.Start();
        Console.WriteLine("Server started");
        while (true)
        {
            Console.WriteLine("Waiting for a client...");
            var client = await _server.AcceptTcpClientAsync();
            _clients.Add(client);
            Console.WriteLine("Client connected");
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using var networkStream = client.GetStream();
        var packetReader = new PacketReader(networkStream);
        Console.WriteLine("Handling client...");

        try
        {
            Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
            while (true)
            {
                Console.WriteLine("Waiting for client data...");
                var opCode = await packetReader.ReadOpcodeAsync();
                Console.WriteLine($"Opcode received: {opCode}");
                var message = await packetReader.ReadStringAsync();
                Console.WriteLine($"Received message: {message}");

                foreach (var connectedClient in _clients)
                {
                    var stream = connectedClient.GetStream();
                    var packetBuilder = new PacketBuilder();
                    packetBuilder.WriteOpCode(opCode);
                    packetBuilder.WriteString(message);
                    var packetBytes = packetBuilder.GetPacketBytes();
                    await stream.WriteAsync(packetBytes, 0, packetBytes.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _clients.Remove(client);
            Console.WriteLine("Client disconnected");
            client.Close();
        }
    }
}
