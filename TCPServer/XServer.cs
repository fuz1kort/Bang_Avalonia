using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using TCPServer.Services;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace TCPServer;

internal class XServer
{
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    private static readonly List<ConnectedClient> ConnectedClients = new();

    private bool _listening;
    private bool _stopListening;
    private bool _isGameOver;

    private static Stack<byte> _cardsDeck = new();
    private static Stack<string?> _heroesDeck = new();
    private static Stack<byte> _rolesDeck = new();
    // private static Stack<PlayCard> _reset = new();

    private int _activePlayerId;

    public Task StartAsync()
    {
        try
        {
            if (_listening)
                throw new Exception("Server is already listening incoming requests.");

            _socket.Bind(new IPEndPoint(IPAddress.Any, 1410));
            _socket.Listen(4);

            _listening = true;

            Console.WriteLine("Server has been started");
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
        while (true)
        {
            if (_stopListening)
                return;
            Socket client;

            try
            {
                client = _socket.Accept();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(10000);
                continue;
            }

            Console.WriteLine($"[!] Accepted client from {(IPEndPoint)client.RemoteEndPoint!}");

            var c = new ConnectedClient(client, (byte)(ConnectedClients.Count));

            ConnectedClients.Add(c);

            c.PropertyChanged += Client_PropertyChanged!;

            if (ConnectedClients.Count == 2)
                break;
        }
    }

    private static void Client_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var client = sender as ConnectedClient;
        foreach (var connectedClient in ConnectedClients)
        {
            var id = client!.Id;
            var propertyName = e.PropertyName;
            var type = client.GetType();
            var property = type.GetProperty(e.PropertyName!);
            var value = property!.GetValue(client);
            connectedClient.Update(id, propertyName, value);
        }
    }

    private static void InitializeGame()
    {
        var (rolesDeck, heroesDeck, cardsDeck) = new GeneratorService().GenerateDecks();
        _rolesDeck = rolesDeck;
        _heroesDeck = heroesDeck;
        _cardsDeck = cardsDeck;
    }

    public void StartGame()
    {
        InitializeGame();

        while (true)
        {
            if (!ConnectedClients.All(x => x.IsReady)) continue;
            Thread.Sleep(100);
            break;
        }

        foreach (var client in ConnectedClients)
        {
            var hero = _heroesDeck.Pop();
            var role = _rolesDeck.Pop();
            client.HeroName = hero;
            client.RoleType = role;
            Thread.Sleep(1000);
            var hp = client.Hp;
            var cards = new List<byte>();
            for (var i = 0; i < hp; i++)
                cards.Add(_cardsDeck.Pop());
            client.Cards = cards;

            // if (role == 0)
            //     _activePlayerId = Clients.IndexOf(client);
        }

        _activePlayerId = 0;

        _isGameOver = false;

        while (!_isGameOver)
        {
            var activePlayer = ConnectedClients[_activePlayerId % 4];
            //switch (activePlayer.HeroName)
            //{
            //    case "Туко":
            //        //TODO
            //        break;
            //    case "Джесси Джеймс":
            //        //TODO
            //        break;
            //    case "Кит Карсон":
            //        //TODO
            //        break;
            //    case "Бешенный Пёс":
            //        //TODO
            //        break;
            //    default:
            var cards = new List<byte>
            {
                _cardsDeck.Pop(),
                _cardsDeck.Pop()
            };

            activePlayer.Turn = true;

            cards.AddRange(activePlayer.Cards!);

            activePlayer.Cards = cards;
            //break;
            //}

            var activePlayerName = activePlayer.Name;
            Console.WriteLine($"{activePlayerName}'s turn");
            while (true)
            {
                if (!activePlayer.Turn)
                    break;
            }

            Console.WriteLine($"Player {activePlayer.Name} has finished his turn");
            _activePlayerId += 1;
        }
    }
}