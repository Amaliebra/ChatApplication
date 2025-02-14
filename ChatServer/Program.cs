
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
        var program = new Program("172.16.17.197", 5000);
        await program.StartAsync();
    }

    public async Task StartAsync()
    {
        _server.Start();
        Console.WriteLine("Server started");

        while (true)
        {
            Console.WriteLine("Waiting for a client...");
            var tcpClient = await _server.AcceptTcpClientAsync();


            //if (_clients.Any(c => ((IPEndPoint)c.ClientSocket.Client.RemoteEndPoint).Address.Equals( 
            //                       ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address)))
            //{
            //    Console.WriteLine("Duplicate client detected, rejecting connection.");
            //    tcpClient.Close();
            //    continue;
            //}


            //need to uncomment this after testing
            var clientWrapper = new Client(tcpClient);
            clientWrapper.DirectMessageReceived += HandleDirectMessage;
            clientWrapper.Disconnected += HandleClientDisconnected;

            await clientWrapper.InitializationTask;
            Console.WriteLine("Adding clients to _clients list");
            _clients.Add(clientWrapper);
            Console.WriteLine($"Client added. Total clients: {_clients.Count}");

            Console.WriteLine("Before broadcast");
            await BroadcastUser();
            Console.WriteLine("User list broadcast completed");

            Console.WriteLine($"Client connected: {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address}");
        }
    }

    
    private void HandleDirectMessage(Client sender, string recipient, string messageText)
    {
        Console.WriteLine($"Direct message from {sender.Username} to {recipient}: {messageText}");

        var targetClient = _clients.FirstOrDefault(c => c.Username == recipient && c.ClientSocket.Connected);

        if (targetClient != null)
        {

            var forwardPacket = new PacketBuilder();
            forwardPacket.WriteOpCode(5);

            forwardPacket.WriteString(sender.Username);
            forwardPacket.WriteString(messageText);
            var packetBytes = forwardPacket.GetPacketBytes();

            try
            {
                if (targetClient.ClientSocket.Connected)
                {
                    targetClient.ClientSocket.GetStream().WriteAsync(packetBytes, 0, packetBytes.Length);
                }
                else
                {
                    Console.WriteLine($"Client {targetClient.Username} is disconnected.");
                }
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

    private async void HandleClientDisconnected(Client client)
    {
        Console.WriteLine($"{client.Username} disconnected.");
        _clients.Remove(client);
        await BroadcastUser();
    }

    private async Task BroadcastUser()
    {
        foreach (var client in _clients)
        {
      
            var userListForClient = _clients
                .Where(c => c != client && !string.IsNullOrEmpty(c.Username?.Trim())) 
                .Select(c => c.Username?.Trim())
                .ToList();

            string UserListString = string.Join(",", userListForClient);
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Broadcasting user list to {client.Username}: {UserListString}");

            var packetBuilder = new PacketBuilder();
            packetBuilder.WriteOpCode(2);
            packetBuilder.WriteString(UserListString);
            byte[] packetBytes = packetBuilder.GetPacketBytes();

            try
            {
                if (client.ClientSocket.Connected) 
                {
                    await client.ClientSocket.GetStream().WriteAsync(packetBytes, 0, packetBytes.Length);
                }
                else
                {
                    Console.WriteLine($"[WARNING] Client {client.Username} disconnected, cannot broadcast user list.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error broadcasting user list to {client.Username}: {ex.Message}");
            }
        }
    }

}


