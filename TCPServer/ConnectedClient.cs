using System.ComponentModel;
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

    public byte Id
    {
        get => _id;
        private init
        {
            _id = value;
            OnPropertyChanged();
        }
    }
    private readonly byte _id;
    private string? _colorString;
    private bool _turn;
    private byte _hp;
    private List<byte>? _cards;
    private List<byte>? _openedCards;
    private string? _name;
    private bool _isSheriff;
    private byte? _roleType;
    private string? _heroName;

    public string? Name
    {
        get => _name;
        private set
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
            Update(Id, nameof(RoleType), _roleType);
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

    public List<byte>? OpenedCards
    {
        get => _openedCards;
        set
        {
            _openedCards = value;
            OnPropertyChanged();
        }
    }

    public List<byte>? Cards
    {
        get => _cards;
        set
        {
            _cards = value;
            QueuePacketSend(XPacketConverter.Serialize(XPacketType.Cards, new XPacketBytesList(_cards!)).ToPacket());
        }
    }

    public bool Turn
    {
        get => _turn;
        set
        {
            _turn = value;
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
            var buff = new byte[512];
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
            case XPacketType.Cards:
                //TODO сделать получение карт
                break;
            case XPacketType.Id:
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessUpdatingProperty(XPacket packet)
    {
        var packetProperty = XPacketConverter.Deserialize<XPacketUpdatedPlayerProperty>(packet);
        var property = typeof(ConnectedClient).GetProperty(packetProperty.PropertyName!);
        property!.SetValue(this, Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!));
        OnPropertyChanged(property.Name);
    }

    private void ProcessEndTurn() => Turn = false;

    private void ProcessConnection(XPacket packet)
    {
        var packetConnection = XPacketConverter.Deserialize<XPacketConnection>(packet);
        packetConnection.IsSuccessful = true;

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Connection, packetConnection).ToPacket());

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Id,
            new XPacketId(Id)).ToPacket());

        var colorsCount = ColorsList.Count;
        var randomNum = _random.Next(colorsCount);
        ColorString = ColorsList[randomNum];
        ColorsList.RemoveAt(randomNum);
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

            Client.Send(encryptedPacket);

            Thread.Sleep(100);
        }
    }

    internal void Update(byte id, string? objectName, object? obj)
        => QueuePacketSend(XPacketConverter.Serialize(XPacketType.UpdatedPlayerProperty,
                new XPacketUpdatedPlayerProperty(id, objectName, Type.GetType(obj!.GetType().ToString())!, obj))
            .ToPacket());
}