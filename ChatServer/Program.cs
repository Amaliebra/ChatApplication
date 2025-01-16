
using System.Net;
using System.Net.Sockets;

public class ChatServerProgram
{
    private TcpListener _server;
    private List<TcpClient> _clients;

    public ChatServerProgram(string ip, int port)
    {
        _server = new TcpListener(IPAddress.Any, port);
        _clients = new List<TcpClient>();
    }

    public async Task StartAsync()
    {
        _server.Start();
        Console.WriteLine("Server started");
        while (true)
        {
            var client = await _server.AcceptTcpClientAsync();
            _clients.Add(client);
            Console.WriteLine("Client connected");
            Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        throw new NotImplementedException();
    }
}
