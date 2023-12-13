using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Bang_Cards_Models;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace Bang_Game.Models;

public sealed class Player : INotifyPropertyChanged
{
    private byte _id;

    public byte Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged();
        }
    }

    private string? _name;

    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    private string? _colorString;

    public string? ColorString
    {
        get => _colorString;
        set
        {
            _colorString = value;
            OnPropertyChanged();
        }
    }

    private RoleCard? _role;

    public RoleCard? Role
    {
        get => _role;
        set
        {
            _role = value;
            OnPropertyChanged();
        }
    }

    private HeroCard? _hero;

    public HeroCard? Hero
    {
        get => _hero;
        set
        {
            _hero = value;
            OnPropertyChanged();
        }
    }

    private List<PlayCard>? _cards;

    public List<PlayCard>? Cards
    {
        get => _cards;
        set
        {
            _cards = value;
            Console.WriteLine("Карты получены");
            OnPropertyChanged();
        }
    }

    private bool _turn;

    public bool Turn
    {
        get => _turn;
        set
        {
            _turn = value;
            Console.WriteLine("Ваш ход");
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private Player(byte id, string? name, uint rgb)
    {
        Id = id;
        Name = name;
        ColorString = Color.FromUInt32(rgb).ToString();
    }

    public Player()
    {
    }

    private ObservableCollection<Player>? _playersList;

    public ObservableCollection<Player>? PlayersList
    {
        get => _playersList;
        set
        {
            if (value == null) return;
            _playersList = value;
            OnPropertyChanged();
        }
    }

    private readonly Queue<byte[]> _packetSendingQueue = new();

    private Socket? _socket;
    private IPEndPoint? _serverEndPoint;

    internal Task ConnectAsync()
    {
        try
        {
            ConnectAsync("127.0.0.1", 1410);


            QueuePacketSend(XPacketConverter.Serialize(XPacketType.Connection,
                new XPacketConnection
                {
                    IsSuccessful = false
                }).ToPacket());

            Thread.Sleep(100);

            QueuePacketSend(XPacketConverter.Serialize(XPacketType.NewPlayer, new XPacketNewPlayer(Name))
                .ToPacket());

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

    private async Task ReceivePacketsAsync()
    {
        while (true)
        {
            var buff = new byte[1024];
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
                ProcessCreatingPlayer(packet);
                break;
            case XPacketType.Players:
                ProcessGettingPlayers(packet);
                break;
            case XPacketType.BeginSet:
                ProcessGettingBeginSet(packet);
                break;
            case XPacketType.Turn:
                ProcessStartingTurn();
                break;
            case XPacketType.Unknown:
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessStartingTurn()
        => Turn = true;

    private void ProcessGettingBeginSet(XPacket packet)
    {
        var packetBeginSet = XPacketConverter.Deserialize<XPacketBeginSet>(packet);
        Role = packetBeginSet.RoleCard;
        Hero = packetBeginSet.HeroCard;
        var cards = new List<PlayCard>();
        cards.AddRange(packetBeginSet.Cards12!);
        if(packetBeginSet.Cards34!.Count == 2)
            cards.AddRange(packetBeginSet.Cards34!);
        else
            cards.Add(packetBeginSet.Cards34[0]);
        
        if(packetBeginSet.Card5 != null)
            cards.Add(packetBeginSet.Card5);
        Cards = cards;
        Turn = false;
    }

    private void ProcessGettingPlayers(XPacket packet)
    {
        var packetPlayer = XPacketConverter.Deserialize<XPacketPlayers>(packet);
        var playersFromPacket = packetPlayer.Players;
        var playersList = playersFromPacket!.Select(x => new Player(x.Item1, x.Item2, x.Item3)).ToList();
        PlayersList = new ObservableCollection<Player>(playersList);
    }

    private void ProcessConnection(XPacket packet)
    {
        var connection = XPacketConverter.Deserialize<XPacketConnection>(packet);

        if (connection.IsSuccessful) Console.WriteLine("Handshake successful!");
    }

    private void ProcessCreatingPlayer(XPacket packet)
    {
        var packetPlayer = XPacketConverter.Deserialize<XPacketNewPlayer>(packet);
        var newColorUint = packetPlayer.Rgb;
        var color = Color.FromUInt32(newColorUint);
        ColorString = color.ToString();

        Console.WriteLine($"Your Nickname is {Name}");
        Console.WriteLine($"Your color is {ColorString}");
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
            {
                Thread.Sleep(100);
                continue;
            }

            var packet = _packetSendingQueue.Dequeue();
            await _socket!.SendAsync(packet);

            await Task.Delay(100);
        }
    }

    internal void EndTurn()
    {
        Turn = false;
        var packet = XPacketConverter.Serialize(XPacketType.Turn, new XPacketTurn()).ToPacket();
        QueuePacketSend(packet);
    }
}