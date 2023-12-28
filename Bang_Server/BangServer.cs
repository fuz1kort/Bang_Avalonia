using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using Bang_Game_Models;
using Bang_Game_Models.Cards;
using Bang_Server.Services;

namespace Bang_Server;

internal class BangServer
{
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    internal static readonly List<ConnectedClient> ConnectedClients = new();

    private bool _listening;
    private bool _stopListening;
    private bool _isGameOver;

    internal static Dictionary<byte, RoleCard> RoleCards = new();
    internal static Dictionary<string, HeroCard> HeroCards = new();
    internal static Dictionary<byte, PlayCard> PlayCards = new();

    private static Stack<byte> _cardsDeck = new();
    private static Stack<string?> _heroesDeck = new();
    private static Stack<byte> _rolesDeck = new();
    private static Stack<byte> _reset = new();

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

            var c = new ConnectedClient(client, (byte)ConnectedClients.Count);

            ConnectedClients.Add(c);


            c.PropertyChanged += Client_PropertyChanged!;

            if (ConnectedClients.Count == 4)
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
            connectedClient.UpdatePlayerProperty(id, propertyName, value);
        }
    }

    private static void InitializeGame()
    {
        var (rolesDeck, heroesDeck, cardsDeck) = new GeneratorService().GenerateDecks();
        _rolesDeck = rolesDeck;
        _heroesDeck = heroesDeck;
        _cardsDeck = cardsDeck;

        PlayCards = CardsGenerator.GeneratePlayCards();
        HeroCards = CardsGenerator.GenerateHeroCards();
        RoleCards = CardsGenerator.GenerateRoleCards();
    }

    private static void SendCard(ConnectedClient connectedClient)
    {
        if (_cardsDeck.Count == 0)
        {
            _cardsDeck = new GeneratorService().GetNewDeck(_reset);
            _reset = new Stack<byte>();
        }

        var card = _cardsDeck.Pop();
        connectedClient.GiveCard(card);
    }

    public void StartGame()
    {
        InitializeGame();

        while (true)
        {
            if (!ConnectedClients.All(x => x.IsReady))
            {
                Thread.Sleep(1000);
                continue;
            }

            break;
        }

        foreach (var client in ConnectedClients)
        {
            var hero = _heroesDeck.Pop();
            var role = _rolesDeck.Pop();
            client.HeroName = hero;
            client.RoleType = role;

            client.Hp = HeroCards[hero!].HeroHp;
            if (role == (byte)RoleType.Sheriff)
            {
                _activePlayerId = ConnectedClients.IndexOf(client);
                client.IsSheriff = true;
                client.Hp++;
            }

            for (var i = 0; i < client.Hp; i++)
                client.GiveCard(_cardsDeck.Pop());
        }

        _isGameOver = false;

        while (!_isGameOver)
        {
            if (ConnectedClients.Count <= 3)
                break;


            var activePlayer = ConnectedClients[_activePlayerId % 4];

            if (activePlayer.Hp == 0)
            {
                _activePlayerId++;
                continue;
            }

            SendCard(activePlayer);
            
            Thread.Sleep(300);
            
            SendCard(activePlayer);

            activePlayer.StartTurn();

            Console.WriteLine($"{activePlayer.Name}'s turn");
            do
            {
                if (activePlayer.CardOnTable == 0) continue;


                foreach (var client in ConnectedClients)
                    client.IsReady = false;

                var cardId = activePlayer.CardOnTable;
                var card = PlayCards[cardId];
                switch (card.PlayCardType)
                {
                    case PlayCardType.Beer:
                    {
                        activePlayer.Hp += 1;
                        Thread.Sleep(1500);
                        break;
                    }
                    case PlayCardType.Schofield:
                    case PlayCardType.Volcanic:
                    case PlayCardType.Remington:
                    {
                        if (activePlayer.GunCard != 0)
                            SendCardToReset(activePlayer.GunCard);
                        activePlayer.GunCard = cardId;
                        break;
                    }
                    case PlayCardType.Scope:
                    {
                        if (activePlayer.ScopeCard != 0)
                            SendCardToReset(activePlayer.ScopeCard);
                        activePlayer.ShotRange += 1;
                        activePlayer.ScopeCard = cardId;
                        break;
                    }
                    case PlayCardType.Mustang:
                    {
                        if (activePlayer.MustangCard != 0)
                            SendCardToReset(activePlayer.MustangCard);
                        activePlayer.MustangCard = cardId;
                        break;
                    }
                    case PlayCardType.Barrel:
                    {
                        if (activePlayer.BarrelCard != 0)
                            SendCardToReset(activePlayer.BarrelCard);
                        activePlayer.BarrelCard = cardId;
                        break;
                    }
                    case PlayCardType.Stagecoach:
                    {
                        SendCard(activePlayer);
                        SendCard(activePlayer);
                        Thread.Sleep(1000);
                        break;
                    }
                    case PlayCardType.WellsFargo:
                    {
                        SendCard(activePlayer);
                        SendCard(activePlayer);
                        SendCard(activePlayer);
                        Thread.Sleep(1000);
                        break;
                    }
                    case PlayCardType.Saloon:
                    {
                        foreach (var connectedClient in ConnectedClients)
                            connectedClient.UpdatePlayerProperty(connectedClient.Id, nameof(connectedClient.Hp),
                                (byte)(connectedClient.Hp + 1));
                        Thread.Sleep(1500);
                        break;
                    }
                    case PlayCardType.Bang:
                    {
                        var id = activePlayer.ToPlayerId;
                        var player = ConnectedClients[id];
                        if (player.BarrelCard != 0)
                        {
                            var newCard = _cardsDeck.Pop();
                            if (PlayCards[newCard].CardType == CardType.Hearts)
                            {
                                SendCardToReset(player.BarrelCard);
                                player.BarrelCard = 0;
                            }

                            SendCardToReset(newCard);
                        }
                        else if (player.Cards!.Select(x => PlayCards[x].PlayCardType)
                                 .Any(x => x == PlayCardType.Missed))
                        {
                            var missedCard = player.Cards!.Select(x => PlayCards[x])
                                .Where(x => x.PlayCardType == PlayCardType.Missed).ToList()[0];
                            SendCardToReset(missedCard.Id);
                            player.RemoveCard(missedCard.Id);
                        }
                        else if (player.Hp == 1 &&
                                 player.Cards!.Any(x => PlayCards[x].PlayCardType == PlayCardType.Beer))
                        {
                            var beerCard = player.Cards!.Select(x => PlayCards[x])
                                .Where(x => x.PlayCardType == PlayCardType.Beer).ToList()[0];
                            SendCardToReset(beerCard.Id);
                            player.RemoveCard(beerCard.Id);
                        }
                        else
                            player.Hp--;

                        break;
                    }
                    case PlayCardType.Panic:
                    {
                        var takenCard = ConnectedClients[activePlayer.ToPlayerId].GetRandomCard();
                        activePlayer.GiveCard(takenCard);
                        break;
                    }
                    case PlayCardType.CatBalou:
                    {
                        var takenCard = ConnectedClients[activePlayer.ToPlayerId].GetRandomCard();
                        SendCardToReset(takenCard);
                        break;
                    }
                    case PlayCardType.Gatling:
                    {
                        foreach (var player in ConnectedClients.Where(client =>
                                     client.Id != _activePlayerId && client.Hp > 0))
                        {
                            if (player.BarrelCard != 0)
                            {
                                var newCard = _cardsDeck.Pop();
                                if (PlayCards[newCard].CardType == CardType.Hearts)
                                {
                                    SendCardToReset(player.BarrelCard);
                                    player.BarrelCard = 0;
                                }

                                SendCardToReset(newCard);
                            }
                            else if (player.Cards!.Select(x => PlayCards[x].PlayCardType)
                                     .Any(x => x == PlayCardType.Missed))
                            {
                                var missedCard = player.Cards!.Select(x => PlayCards[x])
                                    .Where(x => x.PlayCardType == PlayCardType.Missed).ToList()[0];
                                SendCardToReset(missedCard.Id);
                                player.RemoveCard(missedCard.Id);
                            }
                            else if (player.Hp == 1 &&
                                     player.Cards!.Any(x => PlayCards[x].PlayCardType == PlayCardType.Beer))
                            {
                                var beerCard = player.Cards!.Select(x => PlayCards[x])
                                    .Where(x => x.PlayCardType == PlayCardType.Beer).ToList()[0];
                                SendCardToReset(beerCard.Id);
                                player.RemoveCard(beerCard.Id);
                            }
                            else
                                player.Hp--;
                        }

                        break;
                    }
                    default:
                    {
                        Thread.Sleep(1500);
                        break;
                    }
                }

                _reset.Push(activePlayer.CardOnTable);

                activePlayer.CardOnTable = 0;

                if (activePlayer.IsSheriff &&
                    ConnectedClients.Where(x => x.Id != _activePlayerId).All(x => x.Hp == 0))
                {
                    _isGameOver = true;
                    foreach (var client in ConnectedClients)
                    {
                        if (client.Id == _activePlayerId)
                            activePlayer.Win();
                        else
                            client.Lose();
                    }

                    break;
                }

                if (activePlayer.RoleType == (byte)RoleType.Bandit &&
                    ConnectedClients.Where(x => x.Id != _activePlayerId && x.RoleType != (byte)RoleType.Bandit).All(x => x.Hp == 0))
                {
                    _isGameOver = true;
                    foreach (var client in ConnectedClients)
                    {
                        if (client.RoleType == (byte)RoleType.Bandit)
                            activePlayer.Win();
                        else
                            client.Lose();
                    }

                    break;
                }

                if (activePlayer.RoleType != (byte)RoleType.Renegade ||
                    ConnectedClients.Where(x => x.Id != _activePlayerId).Any(x => x.Hp != 0)) continue;
                {
                    _isGameOver = true;
                    foreach (var client in ConnectedClients)
                    {
                        if (client.Id == _activePlayerId)
                            activePlayer.Win();
                        else
                            client.Lose();
                    }

                    break;
                }
            } while (activePlayer.Turn);

            Console.WriteLine($"Player {activePlayer.Name} has finished his turn");
            _activePlayerId += 1;
        }

        Console.WriteLine("Game over");
    }

    internal static void SendCardToReset(byte id) => _reset.Push(id);
}