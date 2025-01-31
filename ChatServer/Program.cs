
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

            // Prevent duplicate clients (check if they are already connected)
            if (_clients.Any(c => ((IPEndPoint)c.Client.RemoteEndPoint).Address.Equals(((IPEndPoint)client.Client.RemoteEndPoint).Address)))
            {
                Console.WriteLine("Duplicate client detected, rejecting connection.");
                client.Close();
                continue;
            }

            _clients.Add(client);
            Console.WriteLine($"Client connected: {((IPEndPoint)client.Client.RemoteEndPoint).Address}");

            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using var networkStream = client.GetStream();
            var packetReader = new PacketReader(networkStream);
            Console.WriteLine("Handling client...");

            Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");

            _clients.Add(client);
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
        }
        finally
        {
            Console.WriteLine("Removing client from server");
            _clients.RemoveAll(c => c == client);
            client.Close();
        }
    }
}
