using Bang_Game.Models.Cards;
using Bang_Game.Models.Cards.Heroes;
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

    public string? Name { get; set; }

    public string? ColorString { get; set; }

    public byte Hp { get; set; }

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

    public bool IsSheriff { get; }

    public byte CardsCount => (byte)Cards!.Count;

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

    public Player()
    {
        IsSheriff = false;
        PlayersList = new ObservableCollection<Player>();
        Cards = new List<PlayCard>();
        Turn = false;
        byte id = 0;

        var volcanic1 = new PlayCard(id, "Вулканик", 10, CardType.Clubs, PlayCardType.Volcanic, true, 1);
        _playCards[id++] = volcanic1;
        var volcanic2 = new PlayCard(id, "Вулканик", 10, CardType.Spades, PlayCardType.Volcanic, true, 1);
        _playCards[id++] = volcanic2;
        var schofield1 = new PlayCard(id, "Скофилд", 11, CardType.Clubs, PlayCardType.Schofield, true, 2);
        _playCards[id++] = schofield1;
        var schofield2 = new PlayCard(id, "Скофилд", 12, CardType.Clubs, PlayCardType.Schofield, true, 2);
        _playCards[id++] = schofield2;
        var schofield3 = new PlayCard(id, "Скофилд", 13, CardType.Spades, PlayCardType.Schofield, true, 2);
        _playCards[id++] = schofield3;
        var remington = new PlayCard(id, "Ремингтон", 13, CardType.Clubs, PlayCardType.Remington, true, 3);
        _playCards[id++] = remington;
        var carbine = new PlayCard(id, "Карабин", 14, CardType.Clubs, PlayCardType.Carbine, true, 4);
        _playCards[id++] = carbine;
        var winchester = new PlayCard(id, "Винчестер", 8, CardType.Spades, PlayCardType.Winchester, true, 5);
        _playCards[id++] = winchester;

        //Добавление других постоянных карт
        var mustang1 = new PlayCard(id, "Мустанг", 8, CardType.Hearts,
            PlayCardType.Mustang, true);
        _playCards[id++] = mustang1;
        var mustang2 = new PlayCard(id, "Мустанг", 9, CardType.Hearts,
            PlayCardType.Mustang, true);
        _playCards[id++] = mustang2;
        var scope = new PlayCard(id, "Прицел", 14, CardType.Spades,
            PlayCardType.Scope, true);
        _playCards[id++] = scope;
        var barrel1 = new PlayCard(id, "Бочка", 12, CardType.Spades,
            PlayCardType.Barrel, true);
        _playCards[id++] = barrel1;
        var barrel2 = new PlayCard(id, "Бочка", 13, CardType.Spades,
            PlayCardType.Barrel, true);
        _playCards[id++] = barrel2;

        var wellsFargo = new PlayCard(id, "Уэллс Фарго", 3, CardType.Hearts,
            PlayCardType.WellsFargo,
            false);
        _playCards[id++] = wellsFargo;

        var stagecoach1 = new PlayCard(id, "Дилижанс", 9, CardType.Spades,
            PlayCardType.Stagecoach,
            false);
        _playCards[id++] = stagecoach1;

        var stagecoach2 = new PlayCard(id, "Дилижанс", 9, CardType.Spades,
            PlayCardType.Stagecoach,
            false);
        _playCards[id++] = stagecoach2;

        var gatling = new PlayCard(id, "Гатлинг", 10, CardType.Hearts,
            PlayCardType.Gatling,
            false);
        _playCards[id++] = gatling;

        var saloon = new PlayCard(id, "Салун", 5, CardType.Hearts,
            PlayCardType.Saloon,
            false);
        _playCards[id++] = saloon;

        for (var i = 9; i < 12; i++)
        {
            var catBalou = new PlayCard(id, "Красотка", (byte)i, CardType.Diamonds,
                PlayCardType.CatBalou,
                false);
            _playCards[id++] = catBalou;
        }

        var catBalou4 = new PlayCard(id, "Красотка", 13, CardType.Hearts,
            PlayCardType.CatBalou,
            false);
        _playCards[id++] = catBalou4;

        var panic1 = new PlayCard(id, "Паника", 8, CardType.Diamonds,
            PlayCardType.Panic,
            false);
        _playCards[id++] = panic1;
        var panic2 = new PlayCard(id, "Паника", 11, CardType.Hearts,
            PlayCardType.Panic,
            false);
        _playCards[id++] = panic2;
        var panic3 = new PlayCard(id, "Паника", 12, CardType.Hearts,
            PlayCardType.Panic,
            false);
        _playCards[id++] = panic3;
        var panic4 = new PlayCard(id, "Паника", 14, CardType.Hearts,
            PlayCardType.Panic,
            false);
        _playCards[id++] = panic4;

        for (var i = 6; i < 12; i++)
        {
            var beer = new PlayCard(id, "Пиво", (byte)i, CardType.Hearts,
                PlayCardType.Beer,
                false);
            _playCards[id++] = beer;
        }

        for (var i = 10; i < 15; i++)
        {
            var missed = new PlayCard(id, "Мимо!", (byte)i, CardType.Clubs,
                PlayCardType.Missed,
                false);
            _playCards[id++] = missed;
        }

        for (var i = 2; i < 9; i++)
        {
            var missed = new PlayCard(id, "Мимо!", (byte)i, CardType.Spades,
                PlayCardType.Missed,
                false);
            _playCards[id++] = missed;
        }

        var bang1 = new PlayCard(id, "Бэнг!", 14, CardType.Spades,
            PlayCardType.Bang,
            false);
        _playCards[id++] = bang1;

        for (var i = 2; i < 15; i++)
        {
            var bang = new PlayCard(id, "Бэнг!", (byte)i, CardType.Diamonds,
                PlayCardType.Bang,
                false);
            _playCards[id++] = bang;
        }

        for (var i = 2; i < 10; i++)
        {
            var bang = new PlayCard(id, "Бэнг!", (byte)i, CardType.Clubs,
                PlayCardType.Bang,
                false);
            _playCards[id++] = bang;
        }

        for (var i = 12; i < 15; i++)
        {
            var bang = new PlayCard(id, "Бэнг!", (byte)i, CardType.Hearts,
                PlayCardType.Bang,
                false);
            _playCards[id++] = bang;
        }

        var sheriff = new RoleCard(RoleType.Sheriff, true);
        _roleCards[(byte)sheriff.RoleType] = sheriff;
        var bandit1 = new RoleCard(RoleType.Bandit, false);
        _roleCards[(byte)bandit1.RoleType] = bandit1;
        var bandit2 = new RoleCard(RoleType.Bandit, false);
        _roleCards[(byte)bandit2.RoleType] = bandit2;
        var renegade = new RoleCard(RoleType.Renegade, false);
        _roleCards[(byte)renegade.RoleType] = renegade;

        var billy = new Billy();
        _heroCards[billy.HeroName!] = billy;
        var eyes = new Eyes();
        _heroCards[eyes.HeroName!] = eyes;
        var james = new James();
        _heroCards[james.HeroName!] = james;
        var jane = new Jane();
        _heroCards[jane.HeroName!] = jane;
        var joe = new Joe();
        _heroCards[joe.HeroName!] = joe;
        var kit = new Kit();
        _heroCards[kit.HeroName!] = kit;
        var snake = new Snake();
        _heroCards[snake.HeroName!] = snake;
        var tom = new Tom();
        _heroCards[tom.HeroName!] = tom;
        var tuco = new Tuco();
        _heroCards[tuco.HeroName!] = tuco;
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
            case XPacketType.Id:
                ProcessSettingId(packet);
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessSettingId(XPacket packet)
    {
        Id = XPacketConverter.Deserialize<XPacketId>(packet).Id;
        for (var i = 0; i < Id; i++)
        {
            PlayersList!.Add(new Player((byte)i));
        }
        
        PlayersList!.Add(this);
    }

    private void ProcessStartingTurn(XPacket packet) => Turn = true;

    private void ProcessGettingCardsSet(XPacket packet)
    {
        var allCards = XPacketConverter.Deserialize<XPacketBytesList>(packet).BytesList;
        allCards!.AddRange(Cards!.Select(x => x.Id));
        Cards = allCards.Select(x => _playCards[x]).ToList();
    }

    private void ProcessGettingPlayers(XPacket packet)
    {
        var packetPlayer = XPacketConverter.Deserialize<XPacketBytesList>(packet);
        PlayersList = new ObservableCollection<Player>();
        foreach (var id in packetPlayer.BytesList!.Where(id => id != Id))
        {
            PlayersList.Add(new Player(id));
            OnPropertyChanged(nameof(PlayersList));
        }
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
                PlayersList![packetProperty.PlayerId].HeroCard = _heroCards[
                    (string)Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!)!];
                break;
            case "RoleType":
            {
                RoleCard = _roleCards[
                    (byte)Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!)!];
                var hp = HeroCard!.HeroHp;
                if (IsSheriff)
                    hp += 1;
                Hp = hp;

                var hpPacket = new XPacketUpdatedPlayerProperty(Id, nameof(Hp), Hp.GetType(), Hp);
                var updatedPacket = XPacketConverter.Serialize(XPacketType.UpdatedPlayerProperty, hpPacket).ToPacket();
                QueuePacketSend(updatedPacket);
                break;
            }
            default:
                var property = GetType().GetProperty(packetProperty.PropertyName!);
                property!.SetValue(PlayersList![packetProperty.PlayerId],
                    Convert.ChangeType(packetProperty.PropertyValue, packetProperty.PropertyType!));
                OnPropertyChanged(property.Name);
                break;
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