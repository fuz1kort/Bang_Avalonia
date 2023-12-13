using System.Net;
using System.Net.Sockets;
using TCPServer.GameModels;
using TCPServer.Services;

namespace TCPServer;

internal class XServer
{
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    
    internal static readonly List<ConnectedClient> Clients = new();

    private bool _full;
    private bool _listening;
    private bool _stopListening;

    private Stack<ICard> _deck = new();
    private List<IHeroCard> _heroes = new();
    private List<RoleCard> _roles = new();

    public Task StartAsync()
    {
        try
        {
            if (_listening)
                throw new Exception("Server is already listening incoming requests.");

            _socket.Bind(new IPEndPoint(IPAddress.Any, 4910));
            _socket.Listen(10);

            _listening = true;

            Console.WriteLine("Server have been started");
            var stopThread = new Thread(() =>
            {
                while (_listening)
                    if (Console.ReadLine() == "stop")
                        Stop();
            });
            stopThread.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Task.CompletedTask;
    }

    private void Stop()
    {
        if (!_listening)
            throw new Exception("Server is already not listening incoming requests.");
        _stopListening = true;
        _listening = false;
        _socket.Close();
        Console.WriteLine("Server have been stopped");
    }

    public void AcceptClients()
    {
        while (!_full)
        {
            if (_stopListening)
                return;

            Socket client;

            try
            {
                client = _socket.Accept();
            }
            catch
            {
                return;
            }

            Console.WriteLine($"[!] Accepted client from {(IPEndPoint)client.RemoteEndPoint!}");

            var c = new ConnectedClient(client, (byte)Clients.Count);
            Clients.Add(c);
            if (Clients.Count == 4)
                _full = true;
        }

        InitializeGame();
        StartGame();

        while (_full)
        {
            if (Clients.Count < 4)
            {
                _full = false;
            }
        }
    }

    private void InitializeGame()
    {
        var generated = new GeneratorService().GenerateDecks();
        _roles = generated.Item1;
        _heroes = generated.Item2;
        _deck = generated.Item3;
    }


    private void StartGame()
    {
    }
}