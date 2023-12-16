using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using TCPServer.Services;

namespace TCPServer;

internal class XServer
{
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    internal static readonly List<ConnectedClient> Clients = new();

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
            _socket.Listen(10);

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

            var c = new ConnectedClient(client, (byte)Clients.Count);

            Clients.Add(c);
            c.PropertyChanged += Client_PropertyChanged!;
            
            if (Clients.Count == 4)
                break;
        }
    }

    private void Client_PropertyChanged(object sender, PropertyChangedWithValueEventArgs e)
    {
        var client = sender as ConnectedClient;
        for (var i = 0; i < Clients.Count; i++)
        {
            if(i == client!.Id)
                continue;
            
            Clients[i].Update(client.Id ,e.PropertyName, e.Value);
        }
    }

    private static void InitializeGame()
    {
        var (rolesDeck, heroesDeck, cardsDeck) = new GeneratorService().GenerateDecks();
        _rolesDeck = rolesDeck;
        _heroesDeck = heroesDeck;
        _cardsDeck = cardsDeck;
    }

    public Task StartGameAsync()
    {
        InitializeGame();

        while (true)
        {
            if (Clients.All(x => x.IsReady))
                break;
        }
        
        foreach (var client in Clients)
        {
            var role = _rolesDeck.Pop();
            var hero = _heroesDeck.Pop();
            client.SendRoleHero(role, hero);
            Thread.Sleep(500);
            var hp = client.Hp;
            List<byte> cards = new();
            for (var i = 0; i < hp; i++)
                cards.Add(_cardsDeck.Pop());
            if (role == 0)
                _activePlayerId = Clients.IndexOf(client);

            client.SendBeginCardsSet(cards);
        }

        _isGameOver = false;

        while (!_isGameOver)
        {
            var activePlayer = Clients[_activePlayerId % 4];
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

            activePlayer.SendTurnAndCardsDefault(cards);
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

        return Task.CompletedTask;
    }
}