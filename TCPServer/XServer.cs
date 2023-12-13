using System.Net;
using System.Net.Sockets;
using Bang_Cards_Models;
using TCPServer.Services;

namespace TCPServer;

internal class XServer
{
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    internal static readonly List<ConnectedClient> Clients = new();

    private bool _full;
    private bool _listening;
    private bool _stopListening;

    private static Stack<ICard> _cardsDeck = new();
    private static Stack<IHeroCard> _heroesDeck = new();
    private static Stack<RoleCard> _rolesDeck = new();
    private static Stack<ICard> _reset = new();
    private static int _activePlayerId;

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
        _rolesDeck = generated.Item1;
        _heroesDeck = generated.Item2;
        _cardsDeck = generated.Item3;
    }

    private static void StartGame()
    {
        foreach (var client in Clients)
        {
            var role = _rolesDeck.Pop();
            var hero = _heroesDeck.Pop();
            List<ICard> cards = new();
            for (var i = 0; i < 6; i++)
                cards.Add(_cardsDeck.Pop());
            if (role.RoleType is RoleType.Sheriff)
            {
                _activePlayerId = Clients.IndexOf(client);
                client.SendCardSet(role, hero, cards, true);
            }
            
            else
                client.SendCardSet(role, hero, cards, false);
        }
        
        while (true)
        {
            var activePlayer = Clients[_activePlayerId % 4];
            activePlayer.Turn = true;
            //Где-то ходит
            _activePlayerId += 1;
        }
    }
}