
using ChatServer.Net;
using ChatServer.Net.IO;
using System.Net;
using System.Net.Sockets;

public class Program
{
    private TcpListener _server;
    private List<Client> _clients = new List<Client>();

    public Program(string ip, int port)
    {
        _server = new TcpListener(IPAddress.Any, port);
    }

    public static async Task Main(string[] args)
    {
        var program = new Program("127.0.0.1", 5000);
        await program.StartAsync();
    }
    //private Client ClientForTcp(TcpClient tcpClient)
    //{
    //    return _clients.FirstOrDefault(c => c.ClientSocket == tcpClient);
    //}


    public async Task StartAsync()
    {
        _server.Start();
        Console.WriteLine("Server started");

        while (true)
        {
            Console.WriteLine("Waiting for a client...");
            var tcpClient = await _server.AcceptTcpClientAsync();


            if (_clients.Any(c => ((IPEndPoint)c.ClientSocket.Client.RemoteEndPoint).Address.Equals(
                                   ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address)))
            {
                Console.WriteLine("Duplicate client detected, rejecting connection.");
                tcpClient.Close();
                continue;
            }

            var clientWrapper = new Client(tcpClient);
            clientWrapper.DirectMessageReceived += HandleDirectMessage;
            clientWrapper.Disconnected += HandleClientDisconnected;
            _clients.Add(clientWrapper);
            Console.WriteLine($"Client connected: {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address}");

        }
    }

    
    private void HandleDirectMessage(Client sender, string recipient, string messageText)
    {
        Console.WriteLine($"Direct message from {sender.Username} to {recipient}: {messageText}");

        var targetClient = _clients.FirstOrDefault(c => c.Username == recipient);
        if (targetClient != null)
        {

            var forwardPacket = new PacketBuilder();
            forwardPacket.WriteOpCode(5);

            forwardPacket.WriteString(sender.Username);
            forwardPacket.WriteString(messageText);
            var packetBytes = forwardPacket.GetPacketBytes();

            try
            {
                targetClient.ClientSocket.GetStream().WriteAsync(packetBytes, 0, packetBytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to {targetClient.Username}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Recipient {recipient} not found.");
        }
    }

    private void HandleClientDisconnected(Client client)
    {
        Console.WriteLine($"{client.Username} disconnected.");
        _clients.Remove(client);
    }
}

