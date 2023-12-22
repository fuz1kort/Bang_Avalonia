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

    public string Name
    {
        get => _name;
        set
        {
            if (value.Contains(' ')) return;
            _name = value;
            OnPropertyChanged();
        }
    }

    public string ColorString
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

    public RoleCard RoleCard
    {
        get => _roleCard;
        private set
        {
            _roleCard = value;
            OnPropertyChanged();
        }
    }

    public HeroCard HeroCard
    {
        get => _heroCard;
        private set
        {
            _heroCard = value;
            OnPropertyChanged();
        }
    }

    private PlayCard _scopeCard = null!;

    public PlayCard ScopeCard
    {
        get => _scopeCard;
        set
        {
            _scopeCard = value;
            OnPropertyChanged();
        }
    }

    public byte Distance { get; set; }

    private PlayCard _barrelCard = null!;

    public PlayCard BarrelCard
    {
        get => _barrelCard;
        set
        {
            _barrelCard = value;
            OnPropertyChanged();
        }
    }

    private PlayCard _mustangCard = null!;

    public PlayCard MustangCard
    {
        get => _mustangCard;
        set
        {
            _mustangCard = value;
            OnPropertyChanged();
        }
    }

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

    private PlayCard _gunCard = null!;

    public PlayCard GunCard
    {
        get => _gunCard;
        set
        {
            _gunCard = value;
            OnPropertyChanged();
        }
    }

    private byte _shotRange;

    public byte ShotRange
    {
        get => _shotRange;
        set
        {
            _shotRange = value;
            OnPropertyChanged();
        }
    }

    private PlayCard _cardOnTable = null!;

    public PlayCard CardOnTable
    {
        get => _cardOnTable;
        set
        {
            _cardOnTable = value;
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
        Distance = 0;
    }

    public Player()
    {
        PlayersList = new ObservableCollection<Player> { new(0), new(1), new(2), new(3) };
        Name = "";
        CardOnTable = new PlayCard();

        _playCards = CardsGenerator.GeneratePlayCards();
        _heroCards = CardsGenerator.GenerateHeroCards();
        _roleCards = CardsGenerator.GenerateRoleCards();
    }

    private ObservableCollection<Player> _playersList = null!;

    public ObservableCollection<Player> PlayersList
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
    private HeroCard _heroCard = null!;
    private RoleCard _roleCard = null!;
    private byte _cardsCount;
    private byte _hp;
    private string _colorString = null!;
    private bool _isSheriff;
    private string _name = null!;

    public ObservableCollection<PlayCard> Cards { get; set; } = new();

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
                    new XPacketUpdatedPlayerProperty(Id, nameof(Name), Name.GetType(), Name))
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
            var buff = new byte[1024];
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
            case XPacketType.Card:
                ProcessGettingCard(packet);
                break;
            case XPacketType.Turn:
                ProcessStartingTurn(packet);
                break;
            case XPacketType.Unknown:
                break;
            case XPacketType.OpenedCard:
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessStartingTurn(XPacket packet) => Turn = true;

    private void ProcessGettingCard(XPacket packet)
    {
        var packetCard = XPacketConverter.Deserialize<XPacketCard>(packet);
        Cards.Add(_playCards[packetCard.CardId]);
        CardsCount++;
        OnPropertyChanged(nameof(Cards));
    }

    private void ProcessGettingPlayers(XPacket packet)
    {
        var packetPlayer = XPacketConverter.Deserialize<XPacketPlayers>(packet);
        var playersFromPacket = packetPlayer.Players;
        var playersList = playersFromPacket!.Select(x => new Player(x.Item1, x.Item2, x.Item3)).ToList();
        foreach (var player in playersList)
        {
            PlayersList[player.Id] = playersList[player.Id];
            OnPropertyChanged(nameof(PlayersList));
        }

        playersList[Id] = this;
        OnPropertyChanged(nameof(PlayersList));
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
                PlayersList[packetProperty.PlayerId].HeroCard = _heroCards[(packetProperty.PropertyValue as string)!];
                if (packetProperty.PlayerId == Id)
                    HeroCard = PlayersList[packetProperty.PlayerId].HeroCard;
                break;
            }
            case "RoleType":
            {
                RoleCard = _roleCards[
                    (byte)Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!)!];
                var hp = PlayersList[packetProperty.PlayerId].HeroCard.HeroHp;
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
                ColorString = (packetProperty.PropertyValue as string)!;
                break;
            }
            case "CardOnTable":
            {
                var value = (byte)Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!)!;
                CardOnTable = value == 0 ? new PlayCard() : _playCards[value];
                break;
            }
            case "ScopeCard":
            case "BarrelCard":
            case "MustangCard":
            case "GunCard":
            {
                var property = GetType().GetProperty(packetProperty.PropertyName!);
                property!.SetValue(PlayersList[packetProperty.PlayerId],
                    _playCards[(byte)Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!)!]);
                OnPropertyChanged(nameof(PlayersList));
                if (packetProperty.PlayerId == Id)
                {
                    property.SetValue(this,
                        _playCards[
                            (byte)Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!)!]);
                    OnPropertyChanged(property.Name);
                }

                break;
            }
            default:
            {
                var property = GetType().GetProperty(packetProperty.PropertyName!);
                property!.SetValue(PlayersList[packetProperty.PlayerId],
                    Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!));
                OnPropertyChanged(nameof(PlayersList));
                if (packetProperty.PlayerId == Id)
                {
                    property.SetValue(this,
                        Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!)!);
                    OnPropertyChanged(property.Name);
                }

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

            if (encryptedPacket.Length > 1024)
                throw new Exception("Max packet size is 1024 bytes.");

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

    internal void DropCardOnTable(byte id)
    {
        Cards.Remove(_playCards[id]);
        CardsCount--;
        OnPropertyChanged(nameof(Cards));
        var packet = XPacketConverter.Serialize(XPacketType.Card, new XPacketCard(id)).ToPacket();
        QueuePacketSend(packet);
    }
}