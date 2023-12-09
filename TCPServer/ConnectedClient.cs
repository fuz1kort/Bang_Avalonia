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

            if (parsed != null!)
            {
                ProcessIncomingPacket(parsed);
            }
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
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessConnection(XPacket packet)
    {
        var connection = XPacketConverter.Deserialize<XPacketConnection>(packet);
        connection.IsSuccessfull = true;

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Connection, connection).ToPacket());
    }

    private void ProcessBeginPlayer(XPacket packet)
    {
        var xPacketBeginPlayerName = XPacketConverter.Deserialize<XPacketBeginPlayer>(packet);
        Name = xPacketBeginPlayerName.Name!;

        var colorsCount = XServer.Colors.Count;
        var randomNum = _random.Next(colorsCount);
        var randomColor = XServer.Colors[randomNum];
        Argb = randomColor.ToArgb();
        var xPacketBeginPlayerColor = new XPacketBeginPlayer
        {
            ColorRgb = Argb
        };
        XServer.Colors.RemoveAt(randomNum);

        Console.WriteLine($"Connected player with name: {Name}" +
                          $"\nGiven color: {ColorTranslator.FromHtml(Argb.ToString()).Name}");

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.BeginPlayer, xPacketBeginPlayerColor).ToPacket());
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
            {
                Thread.Sleep(100);
                continue;
            }

            var packet = _packetSendingQueue.Dequeue();
            var encryptedPacket = XProtocolEncryptor.Encrypt(packet);
            await Client.SendAsync(encryptedPacket);

            await Task.Delay(100);
        }
    }
}