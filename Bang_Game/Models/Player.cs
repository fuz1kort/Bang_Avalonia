using Bang_Game.Models.Cards;
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
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace Bang_Game.Models;

public sealed class Player : INotifyPropertyChanged
{
    private readonly Dictionary<byte, RoleCard> _roleCards = new();
    private readonly Dictionary<string, HeroCard> _heroCards = new();
    private readonly Dictionary<byte, PlayCard> _playCards = new();

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

    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public string? ColorString
    {
        get => _colorString;
        set
        {
            _colorString = value;
            OnPropertyChanged();
        }
    }

    public byte Hp
    {
        get => _hp;
        set
        {
            _hp = value;
            OnPropertyChanged();
        }
    }

    public RoleCard? RoleCard
    {
        get => _roleCard;
        private set
        {
            _roleCard = value;
            OnPropertyChanged();
        }
    }

    public HeroCard? HeroCard
    {
        get => _heroCard;
        private set
        {
            _heroCard = value;
            OnPropertyChanged();
        }
    }

    private List<PlayCard>? _openedCards;

    public List<PlayCard>? OpenedCards
    {
        get => _openedCards;
        set
        {
            _openedCards = value;
            OnPropertyChanged();
        }
    }

    //Для других игроков

    public bool IsSheriff
    {
        get => _isSheriff;
        set
        {
            _isSheriff = value;
            OnPropertyChanged();
        }
    }

    public byte CardsCount
    {
        get => _cardsCount;
        set
        {
            _cardsCount = value;
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
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private Player(byte id) => Id = id;

    private Player(byte id, string name, string colorString)
    {
        Id = id;
        Name = name;
        ColorString = colorString;
        Cards = new List<PlayCard>();
    }

    public Player()
    {
        IsSheriff = false;
        PlayersList = new ObservableCollection<Player> { new Player(0), new Player(1), new Player(2), new Player(3) };
        Cards = new List<PlayCard>();
        Turn = false;

        _playCards = CardsGenerator.GeneratePlayCards();
        _heroCards = CardsGenerator.GenerateHeroCards();
        _roleCards = CardsGenerator.GenerateRoleCards();
    }

    private ObservableCollection<Player>? _playersList;

    public ObservableCollection<Player>? PlayersList
    {
        get => _playersList;
        set
        {
            _playersList = value;
            OnPropertyChanged();
        }
    }

    private readonly Queue<byte[]> _packetSendingQueue = new();

    private Socket? _socket;
    private IPEndPoint? _serverEndPoint;
    private HeroCard? _heroCard;
    private RoleCard? _roleCard;
    private byte _cardsCount;
    private byte _hp;
    private string? _colorString;
    private bool _isSheriff;
    private string? _name;

    internal void Connect()
    {
        try
        {
            Connect("127.0.0.1", 1410);

            QueuePacketSend(XPacketConverter.Serialize(XPacketType.Connection,
                new XPacketConnection
                {
                    IsSuccessful = false
                }).ToPacket());

            Thread.Sleep(300);

            QueuePacketSend(XPacketConverter.Serialize(XPacketType.UpdatedPlayerProperty,
                    new XPacketUpdatedPlayerProperty(Id, nameof(Name), Name!.GetType(), Name))
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

    private void Connect(string ip, int port) => Connect(new IPEndPoint(IPAddress.Parse(ip), port));

    private void Connect(IPEndPoint? server)
    {
        _serverEndPoint = server;

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(_serverEndPoint!);

        Task.Run(ReceivePackets);
        Task.Run(SendPackets);
    }

    private void ReceivePackets()
    {
        while (true)
        {
            var buff = new byte[512];
            _socket!.Receive(buff);

            var decryptedBuff = XProtocolEncryptor.Decrypt(buff);

            var packetBuff = decryptedBuff.TakeWhile((b, i) =>
            {
                if (b != 0xFF) return true;
                return decryptedBuff[i + 1] != 0;
            }).Concat(new byte[] { 0xFF, 0 }).ToArray();
            var parsed = XPacket.Parse(packetBuff);

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
            case XPacketType.UpdatedPlayerProperty:
                ProcessUpdatingProperty(packet);
                break;
            case XPacketType.PlayersList:
                ProcessGettingPlayers(packet);
                break;
            case XPacketType.Cards:
                ProcessGettingCardsSet(packet);
                break;
            case XPacketType.Turn:
                ProcessStartingTurn(packet);
                break;
            case XPacketType.Unknown:
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessStartingTurn(XPacket packet) => Turn = true;

    private void ProcessGettingCardsSet(XPacket packet)
    {
        var packetBeginCardsSet = XPacketConverter.Deserialize<XPacketBytesList>(packet);
        var packetCards = packetBeginCardsSet.BytesList;
        foreach (var packetCardId in packetCards!)
        {
            Cards!.Add(_playCards[packetCardId]);
            OnPropertyChanged(nameof(Cards));
            CardsCount++;
        }
    }

    private void ProcessGettingPlayers(XPacket packet)
    {
        var packetPlayer = XPacketConverter.Deserialize<XPacketPlayers>(packet);
        var playersFromPacket = packetPlayer.Players;
        var playersList = playersFromPacket!.Select(x => new Player(x.Item1, x.Item2, x.Item3)).ToList();
        playersList[Id] = this;
        PlayersList = new ObservableCollection<Player>(playersList);
    }

    private static void ProcessConnection(XPacket packet)
    {
        var connection = XPacketConverter.Deserialize<XPacketConnection>(packet);

        if (connection.IsSuccessful)
            Console.WriteLine("Handshake successful!");
    }

    private void ProcessUpdatingProperty(XPacket packet)
    {
        var packetProperty = XPacketConverter.Deserialize<XPacketUpdatedPlayerProperty>(packet);

        switch (packetProperty.PropertyName!)
        {
            case "HeroName":
            {
                PlayersList![packetProperty.PlayerId].HeroCard = _heroCards[
                    (string)Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!)!];
                break;
            }
            case "RoleType":
            {
                RoleCard = _roleCards[
                    (byte)Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!)!];
                var hp = PlayersList![packetProperty.PlayerId].HeroCard!.HeroHp;
                if (RoleCard.RoleType == RoleType.Sheriff)
                    IsSheriff = true;

                if (IsSheriff)
                    hp += 1;
                Hp = hp;

                var hpPacket = new XPacketUpdatedPlayerProperty(Id, nameof(Hp), Hp.GetType(), Hp);
                var updatedPacket = XPacketConverter.Serialize(XPacketType.UpdatedPlayerProperty, hpPacket).ToPacket();
                QueuePacketSend(updatedPacket);
                break;
            }
            case "Id":
            {
                Id = (byte)Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!)!;
                break;
            }
            case "ColorString":
            {
                ColorString = packetProperty.PropertyValue as string;
                break;
            }
            default:
            {
                var property = GetType().GetProperty(packetProperty.PropertyName!);
                property!.SetValue(PlayersList![packetProperty.PlayerId],
                    Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!));
                OnPropertyChanged(nameof(PlayersList));
                break;
            }
        }
    }

    private void QueuePacketSend(byte[] packet)
        => _packetSendingQueue.Enqueue(packet);

    private void SendPackets()
    {
        while (true)
        {
            if (_packetSendingQueue.Count == 0)
                continue;

            var packet = _packetSendingQueue.Dequeue();
            var encryptedPacket = XProtocolEncryptor.Encrypt(packet);

            if (encryptedPacket.Length > 512)
                throw new Exception("Max packet size is 512 bytes.");

            _socket!.Send(encryptedPacket);

            Thread.Sleep(100);
        }
    }

    internal void EndTurn()
    {
        Turn = false;
        var packet = XPacketConverter.Serialize(XPacketType.Turn, new XPacketMovingTurn()).ToPacket();
        QueuePacketSend(packet);
    }
}