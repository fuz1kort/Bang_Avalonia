using System.Drawing;
using System.Net.Sockets;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace TCPServer;

internal class ConnectedClient
{
    private Socket Client { get; }

    private readonly Queue<byte[]> _packetSendingQueue = new();
    
    private static List<Color> Colors = new() { Color.Red, Color.Yellow, Color.Green, Color.Blue };

    private readonly Random _random = new();

    private string Name { get; set; }

    private int Argb { get; set; }

    public ConnectedClient(Socket client)
    {
        Client = client;

        Name = "NoName";
        Argb = 128128128;

        Task.Run(ReceivePacketsAsync);
        Task.Run(SendPacketsAsync);
    }

    private async Task ReceivePacketsAsync()
    {
        while (true) // Слушаем пакеты, пока клиент не отключится.
        {
            var buff = new byte[128]; // Максимальный размер пакета - 128 байт.
            await Client.ReceiveAsync(buff);
            var decrBuff = XProtocolEncryptor.Decrypt(buff);

            buff = decrBuff.TakeWhile((b, i) =>
            {
                if (b != 0xFF) return true;
                return decrBuff[i + 1] != 0;
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
            case XPacketType.BeginPlayer:
                ProcessBeginPlayer(packet);
                break;
            case XPacketType.Unknown:
                break;
            // case XPacketType.Players:
            //     break;
            case XPacketType.Players:
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessConnection(XPacket packet)
    {
        var connection = XPacketConverter.Deserialize<XPacketConnection>(packet);
        connection.IsSuccessful = true;

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Connection, connection).ToPacket());
    }

    private void ProcessBeginPlayer(XPacket packet)
    {
        var xPacketBeginPlayer = XPacketConverter.Deserialize<XPacketBeginPlayer>(packet);
        Name = xPacketBeginPlayer.Name!;

        var colorsCount = Colors.Count;
        var randomNum = _random.Next(colorsCount);
        var randomColor = Colors[randomNum];
        Argb = randomColor.ToArgb();
        xPacketBeginPlayer.Argb = Argb;
        Colors.RemoveAt(randomNum);

        Console.WriteLine($"Connected player with name: {Name}" +
                          $"\nGiven color: {ColorTranslator.FromHtml(Argb.ToString()).Name}");

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.BeginPlayer, xPacketBeginPlayer).ToPacket());
        
        SendPlayers();
    }
    
    private (string, int) GetPlayerParameters() => (Name, Argb);
    
    private static void SendPlayers()
    {
        var players = XServer._clients.Select(x => x.GetPlayerParameters()).ToList();
        var packet = XPacketConverter.Serialize(XPacketType.Players,
            new XPacketPlayers(players: players));
        var bytePacket = packet.ToPacket();
        foreach (var client in XServer._clients) 
            client.QueuePacketSend(bytePacket);
    }

    private void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 128)
            throw new Exception("Max packet size is 128 bytes.");

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
}