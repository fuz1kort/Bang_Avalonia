using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace TCPServer;

internal sealed class ConnectedClient: INotifyPropertyChanged
{
    private Socket Client { get; }

    private readonly Queue<byte[]> _packetSendingQueue = new();

    private static readonly List<string> ColorsList = new()
    {
        "Red", "Yellow", "Green", "Blue"
    };

    private readonly Random _random = new();

    public readonly byte Id;

    public string Name { get; private set; }

    private string? _colorString;
    private bool _turn;
    private byte _hp;
    private List<byte>? _cards;
    private List<byte>? _openedCards;

    private string? ColorString
    {
        get => _colorString;
        set
        {
            _colorString = value;
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
            OnPropertyChanged(_hp);
        }
    }

    public byte? RoleType { get; set; }

    public string? HeroName { get; set; }

    public List<byte>? OpenedCards
    {
        get => _openedCards;
        set
        {
            _openedCards = value;
            OnPropertyChanged(_openedCards!);
        }
    }

    public List<byte>? Cards
    {
        get => _cards;
        set
        {
            _cards = value;
            OnPropertyChanged(_cards!);
        }
    }

    public bool Turn
    {
        get => _turn;
        set
        {
            _turn = value;
            if (value)
            {
                var turnPacket = XPacketConverter.Serialize(XPacketType.Turn, new XPacketMovingTurn());
                var byteTurnPacket = turnPacket.ToPacket();
                QueuePacketSend(byteTurnPacket);
            }
            
            OnPropertyChanged(_turn);
        }
    }

    public bool IsReady { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged(object value, [CallerMemberName] string? propertyName = null) 
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ConnectedClient(Socket client, byte id)
    {
        Name = "DefaultName";
        Client = client;
        Id = id;

        Task.Run(ReceivePacketsAsync);
        Task.Run(SendPacketsAsync);
    }

    private async Task ReceivePacketsAsync()
    {
        while (true)
        {
            var buff = new byte[512];
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
            case XPacketType.UpdatedPlayerProperty:
                ProcessUpdatingProperty(packet);
                break;
            case XPacketType.Turn:
                ProcessEndTurn();
                break;
            case XPacketType.Name:
                ProcessSettingName(packet);
                break;
            case XPacketType.Color:
                break;
            case XPacketType.Unknown:
                break;
            case XPacketType.PlayersForList:
                break;
            case XPacketType.Cards:
                //TODO сделать получение карт
                break;
            case XPacketType.RoleHero:
                break;
            case XPacketType.Hp:
                ProcessUpdatingProperty(packet);
                break;
            case XPacketType.PlayersInfo:
                break;
            case XPacketType.Id:
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessSettingName(XPacket packet)
    {
        var xPacketName = XPacketConverter.Deserialize<XPacketNameOrColor>(packet);
        Name = xPacketName.NameOrColor!;

        var colorsCount = ColorsList.Count;
        var randomNum = _random.Next(colorsCount);
        ColorString = ColorsList[randomNum];
        ColorsList.RemoveAt(randomNum);

        var xPacketColor = new XPacketNameOrColor(ColorString);
        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Color, xPacketColor).ToPacket());
        SendPlayers();
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
        var xPacketConnection = XPacketConverter.Deserialize<XPacketConnection>(packet);
        xPacketConnection.IsSuccessful = true;

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Connection, xPacketConnection).ToPacket());

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Id,
            new XPacketId(Id)).ToPacket());
    }

    private void QueuePacketSend(byte[] packet)
        => _packetSendingQueue.Enqueue(packet);

    private async Task SendPacketsAsync()
    {
        while (true)
        {
            if (_packetSendingQueue.Count == 0)
                continue;

            var packet = _packetSendingQueue.Dequeue();
            var encryptedPacket = XProtocolEncryptor.Encrypt(packet);

            if (encryptedPacket.Length > 512)
                throw new Exception("Max packet size is 512 bytes.");

            await Client.SendAsync(encryptedPacket);

            await Task.Delay(100);
        }
    }

    internal void Update(byte id, string? objectName, object? obj)
    {
        var packet = XPacketConverter.Serialize(XPacketType.UpdatedPlayerProperty,
                new XPacketUpdatedPlayerProperty(id, objectName, Type.GetType(obj!.GetType().ToString())!, obj))
            .ToPacket();
        QueuePacketSend(packet);
    }

    private (byte, string, string) GetPlayerParameters() => (Id, Name, ColorString!);

    private static void SendPlayers()
    {
        var players = XServer.ConnectedClients.Select(x => x.GetPlayerParameters()).ToList();
        var packet = XPacketConverter.Serialize(XPacketType.PlayersForList,
            new XPacketPlayersForList { Players = players });
        var bytePacket = packet.ToPacket();
        foreach (var client in XServer.ConnectedClients)
            client.QueuePacketSend(bytePacket);
    }

    public void SendBeginCardsSet(List<byte> cards)
    {
        Cards = cards;
        var packet = XPacketConverter.Serialize(XPacketType.Cards,
            new XPacketCards(cards));
        var bytePacket = packet.ToPacket();
        QueuePacketSend(bytePacket);
    }

    public void SendStartingCards(List<byte> cards)
    {
        var cardsPacket = XPacketConverter.Serialize(XPacketType.Cards,
            new XPacketCards(cards));
        var byteCardsPacket = cardsPacket.ToPacket();
        QueuePacketSend(byteCardsPacket);
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