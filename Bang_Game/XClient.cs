using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Bang_Game.Models;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace Bang_Game;

public class XClient
{
    private readonly Queue<byte[]> _packetSendingQueue = new();

    private Socket? _socket;
    private IPEndPoint? _serverEndPoint;

    private Player? Player { get; set; }

    public event Action<List<Player>>? PlayersReceivedEvent;

    public async Task ConnectAsync(string name)
    {
        try
        {
            ConnectAsync("127.0.0.1", 4910);


            QueuePacketSend(XPacketConverter.Serialize(XPacketType.Connection,
                new XPacketConnection
                {
                    IsSuccessful = false
                }).ToPacket());

            Thread.Sleep(100);


            Player = new Player(0,name, 0);

            QueuePacketSend(XPacketConverter.Serialize(XPacketType.NewPlayer, new XPacketNewPlayer(name: name))
                .ToPacket());

            await Task.Delay(100);

            while (true)
            {
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void ConnectAsync(string ip, int port) => ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));

    private async Task ConnectAsync(IPEndPoint? server)
    {
        _serverEndPoint = server;

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await _socket.ConnectAsync(_serverEndPoint!);

        Task.Run(ReceivePacketsAsync);
        Task.Run(SendPacketsAsync);
    }

    private void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 512)
            throw new Exception("Max packet size is 512 bytes.");

        _packetSendingQueue.Enqueue(packet);
    }

    private async Task ReceivePacketsAsync()
    {
        while (true)
        {
            var buff = new byte[512];
            await _socket!.ReceiveAsync(buff);

            buff = buff.TakeWhile((b, i) =>
            {
                if (b != 0xFF) return true;
                return buff[i + 1] != 0;
            }).Concat(new byte[] { 0xFF, 0 }).ToArray();
            var parsed = XPacket.Parse(buff);

            if (parsed != null!) ProcessIncomingPacket(parsed);
        }
    }

    private void ProcessIncomingPacket(XPacket packet)
    {
        var type = XPacketTypeManager.GetTypeFromPacket(packet);

        switch (type)
        {
            case XPacketType.Connection:
                ProcessConnection(packet);
                break;
            case XPacketType.NewPlayer:
                ProcessBeginPlayer(packet);
                break;
            case XPacketType.Players:
                ProcessPlayers(packet);
                break;
            case XPacketType.BeginSet:
                ProcessBeginSet(packet);
                break;
            case XPacketType.Unknown:
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessBeginSet(XPacket packet)
    {
        var packetBeginSet = XPacketConverter.Deserialize<XPacketBeginSet>(packet);
        var role = packetBeginSet.RoleCard;
        var hero = packetBeginSet.HeroCard;
        var cards = packetBeginSet.Cards;
        var firstTurn = packetBeginSet.FirstTurn;
        Player!.BeginSet(role!, hero!, cards!, firstTurn);
    }

    private void ProcessPlayers(XPacket packet)
    {
        var packetPlayer = XPacketConverter.Deserialize<XPacketPlayers>(packet);
        var playersFromPacket = packetPlayer.Players;
        var playersList = playersFromPacket!.Select(x => new Player(x.Item1, x.Item2, x.Item3)).ToList();
        PlayersReceivedEvent!.Invoke(playersList);
    }

    private void ProcessConnection(XPacket packet)
    {
        var connection = XPacketConverter.Deserialize<XPacketConnection>(packet);

        if (connection.IsSuccessful) Console.WriteLine("Handshake successful!");
    }

    private void ProcessBeginPlayer(XPacket packet)
    {
        var packetPlayer = XPacketConverter.Deserialize<XPacketNewPlayer>(packet);

        var newColorUint = packetPlayer.Rgb;
        var color = Color.FromUInt32(newColorUint);
        Player!.SetColor(newColorUint);

        Console.WriteLine($"Your Nickname is {Player.Name}");
        Console.WriteLine($"Your color is {color.ToString()}");
    }

    private async Task SendPacketsAsync()
    {
        while (true)
        {
            if (_packetSendingQueue.Count == 0)
            {
                Thread.Sleep(100);
                continue;
            }

            var packet = _packetSendingQueue.Dequeue();
            await _socket!.SendAsync(packet);

            await Task.Delay(100);
        }
    }
}