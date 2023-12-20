﻿using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace TCPServer;

internal sealed class ConnectedClient : INotifyPropertyChanged
{
    private Socket Client { get; }

    private readonly Queue<byte[]> _packetSendingQueue = new();

    private static readonly List<string> ColorsList = new()
    {
        "Red", "Yellow", "Green", "Blue"
    };

    private readonly Random _random = new();

    public readonly byte Id;
    private string? _colorString;
    private bool _turn;
    private byte _hp;
    private string? _name;
    private byte? _roleType;
    private string? _heroName;
    private byte _cardsCount;
    private bool _isSheriff;
    private byte _shotRange;
    private byte _distance;

    public string? Name
    {
        get => _name;
        private set
        {
            _name = value;
            var colorsCount = ColorsList.Count;
            var randomNum = _random.Next(colorsCount);
            ColorString = ColorsList[randomNum];
            QueuePacketSend(XPacketConverter.Serialize(XPacketType.UpdatedPlayerProperty,
                new XPacketUpdatedPlayerProperty(Id, nameof(ColorString),
                    ColorString.GetType(), ColorString)).ToPacket());
            ColorsList.RemoveAt(randomNum);
        }
    }

    public string? ColorString
    {
        get => _colorString;
        set
        {
            _colorString = value;
            SendPlayers();
            Console.WriteLine($"Connected player with name: {Name}" +
                              $"\nGiven color: {_colorString}");
            IsReady = true;
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

    public byte Distance
    {
        get => _distance;
        set
        {
            _distance = value;
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

    public byte? RoleType
    {
        get => _roleType;
        set
        {
            _roleType = value;
            UpdatePlayerProperty(Id, nameof(RoleType), _roleType);
        }
    }

    public string? HeroName
    {
        get => _heroName;
        set
        {
            _heroName = value;
            OnPropertyChanged();
        }
    }

    private List<byte>? Cards { get; init; }

    public byte CardsCount
    {
        get => _cardsCount;
        set
        {
            _cardsCount = value;
            OnPropertyChanged();
        }
    }

    public bool Turn
    {
        get => _turn;
        private set
        {
            _turn = value;
            OnPropertyChanged();
        }
    }

    private byte _activeCard;

    public byte ActiveCard
    {
        get => _activeCard;
        set
        {
            _activeCard = value;
            if(value != 100)
                OnPropertyChanged();
        }
    }

    private byte _scopeCard;

    public byte ScopeCard
    {
        get => _scopeCard;
        set
        {
            _scopeCard = value;
            OnPropertyChanged();
        }
    }

    private byte _barrelCard;

    public byte BarrelCard
    {
        get => _barrelCard;
        set
        {
            _barrelCard = value;
            OnPropertyChanged();
        }
    }

    private byte _mustangCard;

    public byte MustangCard
    {
        get => _mustangCard;
        set
        {
            _mustangCard = value;
            OnPropertyChanged();
        }
    }
    
    private byte _gunCard;

    public byte GunCard
    {
        get => _gunCard;
        set
        {
            _gunCard = value;
            ShotRange = value == 10 ? (byte)1 : (byte)2;
            OnPropertyChanged();
        }
    }

    public byte ShotRange
    {
        get => _shotRange;
        set
        {
            _shotRange = value;
            OnPropertyChanged();
        }
    }

    public bool IsReady { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ConnectedClient(Socket client, byte id)
    {
        Client = client;
        Id = id;
        Cards = new List<byte>();

        Task.Run(ReceivePackets);
        Task.Run(SendPackets);
    }

    private void ReceivePackets()
    {
        while (true)
        {
            var buff = new byte[1024];
            Client.Receive(buff);

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
            case XPacketType.UpdatedPlayerProperty:
                ProcessUpdatingProperty(packet);
                break;
            case XPacketType.Turn:
                ProcessEndTurn();
                break;
            case XPacketType.Unknown:
                break;
            case XPacketType.PlayersList:
                break;
            case XPacketType.Card:
                ProcessGettingActiveCard(packet);
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessGettingActiveCard(XPacket packet)
    {
        var packetCard = XPacketConverter.Deserialize<XPacketCard>(packet);
        Cards!.Remove(packetCard.CardId);
        CardsCount--;
        ActiveCard = packetCard.CardId;
    }

    private void ProcessUpdatingProperty(XPacket packet)
    {
        var packetProperty = XPacketConverter.Deserialize<XPacketUpdatedPlayerProperty>(packet);
        switch (packetProperty.PropertyName)
        {
            case "Name":
            {
                Name = Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!) as string;
                break;
            }
            default:
            {
                var property = typeof(ConnectedClient).GetProperty(packetProperty.PropertyName!);
                property!.SetValue(this,
                    Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!));
                OnPropertyChanged(property.Name);
                break;
            }
        }
    }

    private void ProcessEndTurn() => Turn = false;

    private void ProcessConnection(XPacket packet)
    {
        var packetConnection = XPacketConverter.Deserialize<XPacketConnection>(packet);
        packetConnection.IsSuccessful = true;

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Connection, packetConnection).ToPacket());

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.UpdatedPlayerProperty,
            new XPacketUpdatedPlayerProperty(Id, nameof(Id), Id.GetType(), Id)).ToPacket());
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

            Client.Send(encryptedPacket);

            Thread.Sleep(100);
        }
    }

    internal void UpdatePlayerProperty(byte id, string? objectName, object? obj)
        => QueuePacketSend(XPacketConverter.Serialize(XPacketType.UpdatedPlayerProperty,
                new XPacketUpdatedPlayerProperty(id, objectName, obj!.GetType(), obj))
            .ToPacket());
    
    private (byte, string, string) GetPlayerParameters() => (Id, Name, ColorString!)!;

    private static void SendPlayers()
    {
        var players = XServer.ConnectedClients.Select(x => x.GetPlayerParameters()).ToList();
        var packet = XPacketConverter.Serialize(XPacketType.PlayersList,
            new XPacketPlayers { Players = players });
        var bytePacket = packet.ToPacket();
        foreach (var client in XServer.ConnectedClients)
            client.QueuePacketSend(bytePacket);
    }

    public void GiveCard(byte cardId)
    {
        Cards!.Add(cardId);
        CardsCount++;
        var packerCard = XPacketConverter.Serialize(XPacketType.Card, new XPacketCard(cardId)).ToPacket();
        QueuePacketSend(packerCard);
    }

    public void StartTurn()
    {
        Turn = true;
        var packetTurn = XPacketConverter.Serialize(XPacketType.Turn, new XPacketMovingTurn()).ToPacket();
        QueuePacketSend(packetTurn);
    }
}