using TCPClient;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

Console.Title = "XClient";
Console.ForegroundColor = ConsoleColor.White;

var client = new XClient();
client.Connect("127.0.0.1", 4910);

Console.WriteLine("Sending handshake packet..");

Thread.Sleep(1000);

client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.Handshake,
    new XPacketHandshake
    {
        MagicHandshakeNumber = 14
    }).ToPacket());

Thread.Sleep(1000);

Console.WriteLine("Введите имя");
var name = Console.ReadLine()!;
Console.WriteLine("Введите возраст");
var age = int.Parse(Console.ReadLine()!);

Console.WriteLine("Sending name packet..");

Thread.Sleep(1000);

client.QueuePacketSend(XPacketConverter.Serialize(
    XPacketType.Name, new XPacketName
    {
        Name = name,
        Age = age
    }).ToPacket());


while (true)
{
}