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

public sealed class Player
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

    private byte _hp;

    public byte Hp
    {
        get => _hp;
        set
        {
            _hp = value;
            OnPropertyChanged();
        }
    }

    private RoleCard? _roleCard;

    public RoleCard? RoleCard
    {
        get => _roleCard;
        private set
        {
            _roleCard = value;
            OnPropertyChanged();
        }
    }

    private HeroCard? _heroCard;

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
            Update(nameof(OpenedCards), _openedCards!);
            OnPropertyChanged();
        }
    }

    //Для других игроков

    private bool _isSheriff;

    public bool IsSheriff
    {
        get => _isSheriff;
        set
        {
            _isSheriff = value;
            OnPropertyChanged();
        }
    }

    private byte _cardsCount;

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
            if (_cards != null)
            {
                _cards = value;
                // Update(nameof(Cards), _cards!);
                OnPropertyChanged();
            }
            else
                _cards = value;

            CardsCount = (byte)_cards!.Count;
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

    private Player(byte id, string? name, string colorString)
    {
        Id = id;
        Name = name;
        ColorString = colorString;
    }

    public Player()
    {
        Cards = new List<PlayCard>();
        Turn = false;
        byte id = 0;

        var volcanic1 = new PlayCard(id, "Вулканик", 10, CardType.Clubs, PlayCardType.Volcanic, true, 1);
        _playCards[++id] = volcanic1;
        var volcanic2 = new PlayCard(id, "Вулканик", 10, CardType.Spades, PlayCardType.Volcanic, true, 1);
        _playCards[++id] = volcanic2;
        var schofield1 = new PlayCard(id, "Скофилд", 11, CardType.Clubs, PlayCardType.Schofield, true, 2);
        _playCards[++id] = schofield1;
        var schofield2 = new PlayCard(id, "Скофилд", 12, CardType.Clubs, PlayCardType.Schofield, true, 2);
        _playCards[++id] = schofield2;
        var schofield3 = new PlayCard(id, "Скофилд", 13, CardType.Spades, PlayCardType.Schofield, true, 2);
        _playCards[++id] = schofield3;
        var remington = new PlayCard(id, "Ремингтон", 13, CardType.Clubs, PlayCardType.Remington, true, 3);
        _playCards[++id] = remington;
        var carbine = new PlayCard(id, "Карабин", 14, CardType.Clubs, PlayCardType.Carbine, true, 4);
        _playCards[++id] = carbine;
        var winchester = new PlayCard(id, "Винчестер", 8, CardType.Spades, PlayCardType.Winchester, true, 5);
        _playCards[++id] = winchester;

        //Добавление других постоянных карт
        var mustang1 = new PlayCard(id, "Мустанг", 8, CardType.Hearts,
            PlayCardType.Mustang, true);
        _playCards[++id] = mustang1;
        var mustang2 = new PlayCard(id, "Мустанг", 9, CardType.Hearts,
            PlayCardType.Mustang, true);
        _playCards[++id] = mustang2;
        var scope = new PlayCard(id, "Прицел", 14, CardType.Spades,
            PlayCardType.Scope, true);
        _playCards[++id] = scope;
        var barrel1 = new PlayCard(id, "Бочка", 12, CardType.Spades,
            PlayCardType.Barrel, true);
        _playCards[++id] = barrel1;
        var barrel2 = new PlayCard(id, "Бочка", 13, CardType.Spades,
            PlayCardType.Barrel, true);
        _playCards[++id] = barrel2;

        var wellsFargo = new PlayCard(id, "Уэллс Фарго", 3, CardType.Hearts,
            PlayCardType.WellsFargo,
            false);
        _playCards[++id] = wellsFargo;

        var stagecoach1 = new PlayCard(id, "Дилижанс", 9, CardType.Spades,
            PlayCardType.Stagecoach,
            false);
        _playCards[++id] = stagecoach1;

        var stagecoach2 = new PlayCard(id, "Дилижанс", 9, CardType.Spades,
            PlayCardType.Stagecoach,
            false);
        _playCards[++id] = stagecoach2;

        var gatling = new PlayCard(id, "Гатлинг", 10, CardType.Hearts,
            PlayCardType.Gatling,
            false);
        _playCards[++id] = gatling;

        var saloon = new PlayCard(id, "Салун", 5, CardType.Hearts,
            PlayCardType.Saloon,
            false);
        _playCards[++id] = saloon;

        for (var i = 9; i < 12; i++)
        {
            var catBalou = new PlayCard(id, "Красотка", (byte)i, CardType.Diamonds,
                PlayCardType.CatBalou,
                false);
            _playCards[++id] = catBalou;
        }

        var catBalou4 = new PlayCard(id, "Красотка", 13, CardType.Hearts,
            PlayCardType.CatBalou,
            false);
        _playCards[++id] = catBalou4;

        var panic1 = new PlayCard(id, "Паника", 8, CardType.Diamonds,
            PlayCardType.Panic,
            false);
        _playCards[++id] = panic1;
        var panic2 = new PlayCard(id, "Паника", 11, CardType.Hearts,
            PlayCardType.Panic,
            false);
        _playCards[++id] = panic2;
        var panic3 = new PlayCard(id, "Паника", 12, CardType.Hearts,
            PlayCardType.Panic,
            false);
        _playCards[++id] = panic3;
        var panic4 = new PlayCard(id, "Паника", 14, CardType.Hearts,
            PlayCardType.Panic,
            false);
        _playCards[++id] = panic4;

        for (var i = 6; i < 12; i++)
        {
            var beer = new PlayCard(id, "Пиво", (byte)i, CardType.Hearts,
                PlayCardType.Beer,
                false);
            _playCards[++id] = beer;
        }

        for (var i = 10; i < 15; i++)
        {
            var missed = new PlayCard(id, "Мимо!", (byte)i, CardType.Clubs,
                PlayCardType.Missed,
                false);
            _playCards[++id] = missed;
        }

        for (var i = 2; i < 9; i++)
        {
            var missed = new PlayCard(id, "Мимо!", (byte)i, CardType.Spades,
                PlayCardType.Missed,
                false);
            _playCards[++id] = missed;
        }

        var bang1 = new PlayCard(id, "Бэнг!", 14, CardType.Spades,
            PlayCardType.Bang,
            false);
        _playCards[++id] = bang1;

        for (var i = 2; i < 15; i++)
        {
            var bang = new PlayCard(id, "Бэнг!", (byte)i, CardType.Diamonds,
                PlayCardType.Bang,
                false);
            _playCards[++id] = bang;
        }

        for (var i = 2; i < 10; i++)
        {
            var bang = new PlayCard(id, "Бэнг!", (byte)i, CardType.Clubs,
                PlayCardType.Bang,
                false);
            _playCards[++id] = bang;
        }

        for (var i = 12; i < 15; i++)
        {
            var bang = new PlayCard(id, "Бэнг!", (byte)i, CardType.Hearts,
                PlayCardType.Bang,
                false);
            _playCards[++id] = bang;
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

            Thread.Sleep(300);

            QueuePacketSend(XPacketConverter.Serialize(XPacketType.Name,
                    new XPacketNameOrColor(Name!))
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
            var buff = new byte[512];
            await _socket!.ReceiveAsync(buff);

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
            case XPacketType.Color:
                ProcessSettingColor(packet);
                break;
            case XPacketType.UpdatedPlayerProperty:
                ProcessUpdatingProperty(packet);
                break;
            case XPacketType.PlayersForList:
                ProcessGettingPlayers(packet);
                break;
            case XPacketType.Cards:
                ProcessGettingCardsSet(packet);
                break;
            case XPacketType.Turn:
                ProcessStartingTurn(packet);
                break;
            case XPacketType.RoleHero:
                ProcessGettingRoleHero(packet);
                break;
            case XPacketType.Unknown:
                break;
            case XPacketType.PlayersInfo:
            //TODO
            case XPacketType.Hp:
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }



    private void ProcessGettingRoleHero(XPacket packet)
    {
        var packetHeroName = XPacketConverter.Deserialize<XPacketRoleHero>(packet);
        RoleCard = _roleCards[packetHeroName.RoleType];
        HeroCard = _heroCards[packetHeroName.HeroName!];
        IsSheriff = RoleCard!.RoleType == RoleType.Sheriff;
        var hp = HeroCard!.HeroHp;
        if (IsSheriff)
            hp += 1;
        Hp = hp;
        var hpPacket = new XPacketUpdatedPlayerProperty(Id, nameof(Hp), Hp.GetType(), Hp);
        var updatedPacket = XPacketConverter.Serialize(XPacketType.UpdatedPlayerProperty, hpPacket).ToPacket();
        QueuePacketSend(updatedPacket);
    }

    private void ProcessStartingTurn(XPacket packet) => Turn = true;

    private void ProcessGettingCardsSet(XPacket packet)
    {
        var packetBeginCardsSet = XPacketConverter.Deserialize<XPacketCards>(packet);
        var packetCards = packetBeginCardsSet.Cards;
        foreach (var packetCardId in packetCards!)
        {
            Cards!.Add(_playCards[packetCardId]);
            CardsCount++;
        }
    }

    private void ProcessGettingPlayers(XPacket packet)
    {
        var packetPlayer = XPacketConverter.Deserialize<XPacketPlayersForList>(packet);
        var playersFromPacket = packetPlayer.Players;
        var playersList = playersFromPacket!.Select(x => new Player(x.Item1, x.Item2, x.Item3)).ToList();
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
        var xPacketProperty = XPacketConverter.Deserialize<XPacketUpdatedPlayerProperty>(packet);
        var property = typeof(Player).GetProperty(xPacketProperty.PropertyName!);
        property!.SetValue(this, Convert.ChangeType(xPacketProperty.PropertyValue, xPacketProperty.PropertyType!));
        OnPropertyChanged(property.Name);
    }

    private void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 512)
            throw new Exception("Max packet size is 512 bytes.");

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
            var encryptedPacket = XProtocolEncryptor.Encrypt(packet);
            await _socket!.SendAsync(encryptedPacket);

            await Task.Delay(100);
        }
    }
    
    private void ProcessSettingColor(XPacket packet)
    {
        var xPacket = XPacketConverter.Deserialize<XPacketNameOrColor>(packet);
        ColorString = xPacket.NameOrColor;
    }

    private void Update(string? objectName, object? obj)
    {
        var packet = XPacketConverter.Serialize(XPacketType.UpdatedPlayerProperty,
                new XPacketUpdatedPlayerProperty(Id, objectName, obj!.GetType(), obj))
            .ToPacket();
        QueuePacketSend(packet);
    }

    internal void EndTurn()
    {
        Turn = false;
        var packet = XPacketConverter.Serialize(XPacketType.Turn, new XPacketMovingTurn()).ToPacket();
        QueuePacketSend(packet);
    }
}