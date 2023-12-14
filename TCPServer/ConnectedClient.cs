﻿using System.Net.Sockets;
using Avalonia.Media;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace TCPServer;

internal class ConnectedClient
{
    private Socket Client { get; }

    private readonly Queue<byte[]> _packetSendingQueue = new();

    private static readonly List<Color> ColorsList = new()
    {
        Colors.Red, Colors.Yellow, Colors.Green, Colors.Blue
    };

    private readonly Random _random = new();

    private byte Id { get; }

    private string Name { get; set; }

    public string GetName() => Name;

    private uint Rgb { get; set; }

    private byte RoleType { get; set; }

    private string? HeroName { get; set; }
    
    private byte Hp { get; set; }

    public byte GetHp() => Hp;

    private List<byte>? Cards { get; set; }

    internal bool Turn { get; private set; }

    internal bool IsReady { get; private set; }

    public ConnectedClient(Socket client, byte id)
    {
        Client = client;

        Id = id;
        Name = "NoName";
        Rgb = 128128128;

        Task.Run(ReceivePacketsAsync);
        Task.Run(SendPacketsAsync);
    }

    private async Task ReceivePacketsAsync()
    {
        while (true)
        {
            var buff = new byte[1024];
            await Client.ReceiveAsync(buff);
            
            var decryptedBuff = XProtocolEncryptor.Decrypt(buff);

            buff = decryptedBuff.TakeWhile((b, i) =>
            {
                if (b != 0xFF) return true;
                return decryptedBuff[i + 1] != 0;
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
                ProcessNewPlayer(packet);
                break;
            case XPacketType.Turn:
                ProcessEndTurn();
                break;
            case XPacketType.Unknown:
                break;
            case XPacketType.Players:
                break;
            case XPacketType.BeginCardsSet:
                break;
            case XPacketType.RoleHero:
                break;
            case XPacketType.Hp:
                ProcessSettingHp(packet);
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessSettingHp(XPacket packet)
    {
        var xPacketHp = XPacketConverter.Deserialize<XPacketHp>(packet);
        Hp = xPacketHp.Hp;
    }

    private void ProcessEndTurn() => Turn = false;

    private void ProcessConnection(XPacket packet)
    {
        var xPacketConnection = XPacketConverter.Deserialize<XPacketConnection>(packet);
        xPacketConnection.IsSuccessful = true;

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Connection, xPacketConnection).ToPacket());
    }

    private void ProcessNewPlayer(XPacket packet)
    {
        var xPacketBeginPlayer = XPacketConverter.Deserialize<XPacketNewPlayer>(packet);
        Name = xPacketBeginPlayer.Name!;

        var colorsCount = ColorsList.Count;
        var randomNum = _random.Next(colorsCount);
        var randomColor = ColorsList[randomNum];
        Rgb = randomColor.ToUInt32();
        xPacketBeginPlayer.Rgb = Rgb;
        ColorsList.RemoveAt(randomNum);

        Console.WriteLine($"Connected player with name: {xPacketBeginPlayer.Name}" +
                          $"\nGiven color: {randomColor}");

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.NewPlayer, xPacketBeginPlayer).ToPacket());

        SendPlayers();

        IsReady = true;
    }

    private void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 1024)
            throw new Exception("Max packet size is 1024 bytes.");

        _packetSendingQueue.Enqueue(packet);
    }

    private async Task SendPacketsAsync()
    {
        while (true)
        {
            if (_packetSendingQueue.Count == 0)
                continue;

            var packet = _packetSendingQueue.Dequeue();
            var encryptedPacket = XProtocolEncryptor.Encrypt(packet);
            await Client.SendAsync(encryptedPacket);

            await Task.Delay(100);
        }
    }

    private (byte, string, uint) GetPlayerParameters() => (Id, Name, Rgb);

    private static void SendPlayers()
    {
        var players = XServer.Clients.Select(x => x.GetPlayerParameters()).ToList();
        var packet = XPacketConverter.Serialize(XPacketType.Players,
            new XPacketPlayers { Players = players });
        var bytePacket = packet.ToPacket();
        foreach (var client in XServer.Clients)
            client.QueuePacketSend(bytePacket);
    }

    public void SendBeginCardsSet(List<byte>? cards)
    {
        Cards = cards;
        var packet = XPacketConverter.Serialize(XPacketType.BeginCardsSet,
            new XPacketBeginSetCards(cards!));
        var bytePacket = packet.ToPacket();
        QueuePacketSend(bytePacket);
    }

    public void SendTurn()
    {
        Turn = true;
        var packet = XPacketConverter.Serialize(XPacketType.Turn,
            new XPacketTurn());
        var bytePacket = packet.ToPacket();
        QueuePacketSend(bytePacket);
    }

    public void SendRoleHero(byte roleType, string? heroName)
    {
        RoleType = roleType;
        HeroName = heroName;
        var packet = XPacketConverter.Serialize(XPacketType.RoleHero, new XPacketRoleHero(roleType, heroName));
        var bytePacket = packet.ToPacket();
        QueuePacketSend(bytePacket);
    }
}