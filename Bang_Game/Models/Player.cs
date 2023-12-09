using System;
using System.Drawing;
using System.Threading.Tasks;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace Bang_Game.Models;

public class Player
{
    private static Player _instance = null!;
    private static readonly object Padlock = new();
    private static XClient _client = new();

    public string? Name { get; private set; }
    public Color Color { get; private set; }

    private Player(string name)
    {
        Name = name;
    }

    public static Player Instance
    {
        get
        {
            lock (Padlock)
            {
                if (_instance == null!) _instance = new Player("Default Name");

                return _instance;
            }
        }
    }

    public void SetName(string name)
    {
        lock (Padlock) Name = name;
    }
    
    public void SetColor(Color color)
    {
        lock (Padlock) Color = color;
    }

    public async Task ConnectAsync()
    {
        try
        {
            _client.ConnectAsync("127.0.0.1", 4910);

            Console.WriteLine("Sending handshake packet..");

            _client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.Handshake,
                new XPacketHandshake
                {
                    MagicHandshakeNumber = 14
                }).ToPacket());

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

    public static void SendPacket(byte[] xpacket)
    {
        _client.QueuePacketSend(xpacket);
    }
}