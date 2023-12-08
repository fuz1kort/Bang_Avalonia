using System.Net.Sockets;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace TCPServer;

internal class ConnectedClient
{
    private Socket Client { get; }

    private readonly Queue<byte[]> _packetSendingQueue = new();
    
    public ConnectedClient(Socket client)
    {
        Client = client;

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
            case XPacketType.Handshake:
                ProcessHandshake(packet);
                break;
            case XPacketType.Name:
                ProcessName(packet);
                break;
            case XPacketType.Unknown:
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessHandshake(XPacket packet)
    {
        Console.WriteLine("Recieved handshake packet.");

        var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);
        handshake.MagicHandshakeNumber += 10;

        Console.WriteLine("Answering..");

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Handshake, handshake).ToPacket());
    }

    private void ProcessName(XPacket packet)
    {
        Console.WriteLine("Recieved name packet.");

        var name = XPacketConverter.Deserialize<XPacketName>(packet);

        Console.WriteLine($"Connected player with name: {name.Name}");
        Console.WriteLine("Answering..");

        QueuePacketSend(XPacketConverter.Serialize(XPacketType.Name, name).ToPacket());
    }

    private void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 128)
        {
            throw new Exception("Max packet size is 128 bytes.");
        }

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
            Client.Send(encryptedPacket);

            await Task.Delay(100);
        }
    }
}