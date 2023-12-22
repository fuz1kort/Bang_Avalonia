﻿using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using TCPServer.Models;
using TCPServer.Services;

namespace TCPServer;

internal class XServer
{
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    internal static readonly List<ConnectedClient> ConnectedClients = new();

    private bool _listening;
    private bool _stopListening;
    private bool _isGameOver;

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

            if (ConnectedClients.Count == 1)
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
    }

    private static void SendCard(ConnectedClient connectedClient)
    {
        if (_cardsDeck.Count == 0)
        {
            _cardsDeck = new GeneratorService().GetNewDeck(_reset);
            _reset = new Stack<byte>();
        }
        
        connectedClient.GiveCard(_cardsDeck.Pop());
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

            Thread.Sleep(1000);
            var hp = client.Hp;
            for (var i = 0; i < hp; i++) 
                client.GiveCard(_cardsDeck.Pop());

            if (role != 0)
                continue;

            _activePlayerId = ConnectedClients.IndexOf(client);
            client.IsSheriff = true;
        }

        _isGameOver = false;

        while (!_isGameOver)
        {
            // if (ConnectedClients.Count <= 3)
            //     break;

            
            var activePlayer = ConnectedClients[_activePlayerId % 4];
            
            SendCard(activePlayer);
            SendCard(activePlayer);
            
            activePlayer.StartTurn();
            

            Console.WriteLine($"{activePlayer.Name}'s turn");
            do
            {
                if (activePlayer.CardOnTable == 0) 
                    continue;
                
                var cardId = activePlayer.CardOnTable;
                switch (cardId)
                {
                    case (byte)PlayCardType.Beer:
                    {
                        activePlayer.Hp += 1;
                        Thread.Sleep(5000);
                        break;
                    }
                    case (byte)PlayCardType.Schofield:
                    case (byte)PlayCardType.Volcanic:
                    {
                        activePlayer.GunCard = cardId;
                        Thread.Sleep(3000);
                        break;
                    }
                    case (byte)PlayCardType.Scope:
                    {
                        activePlayer.ShotRange += 1;
                        activePlayer.ScopeCard = cardId;
                        Thread.Sleep(3000);
                        break;
                    }
                    case (byte)PlayCardType.Mustang:
                    {
                        activePlayer.Distance += 1;
                        activePlayer.MustangCard = cardId;
                        Thread.Sleep(3000);
                        break;
                    }
                    case (byte)PlayCardType.Barrel:
                    {
                        activePlayer.BarrelCard = cardId;
                        Thread.Sleep(3000);
                        break;
                    }
                    case (byte)PlayCardType.Stagecoach:
                    {
                        SendCard(activePlayer);
                        SendCard(activePlayer);
                        break;
                    }
                    case (byte)PlayCardType.WellsFargo:
                    {
                        SendCard(activePlayer);
                        SendCard(activePlayer);
                        SendCard(activePlayer);
                        break;
                    }
                    case (byte)PlayCardType.Saloon:
                    {
                        foreach (var connectedClient in ConnectedClients) 
                            connectedClient.UpdatePlayerProperty(connectedClient.Id, nameof(connectedClient.Hp), connectedClient.Hp+1);
                        Thread.Sleep(3000);
                        break;
                    }
                    case (byte)PlayCardType.Bang:
                        //TODO
                    {
                        var id = 0; // переделать
                        if(ConnectedClients.Count/2 - Math.Abs(activePlayer.Id - id)% ConnectedClients.Count/2 + ConnectedClients[id].Distance <= activePlayer.ShotRange) // В метод Bang у Player
                            continue;
                        break;
                    }
                    case (byte)PlayCardType.Missed:
                        //TODO
                    case (byte)PlayCardType.Panic:
                    //TODO
                    case (byte)PlayCardType.CatBalou:
                    //TODO
                    case (byte)PlayCardType.Gatling:
                    //TODO
                    break;
                }
                
                Thread.Sleep(3000);
                
                _reset.Push(activePlayer.CardOnTable);

                activePlayer.CardOnTable = 0;

            } 
            while (activePlayer.Turn);

            Console.WriteLine($"Player {activePlayer.Name} has finished his turn");
            _activePlayerId += 1;
        }
    }
}