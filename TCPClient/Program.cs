using XProtocol;
using XProtocol.Serializer;

namespace TCPClient
{
    internal class Program
    {
        private static int _handshakeMagic;

        private static void Main()
        {
            Console.Title = "XClient";
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Введите имя");
            var name = Console.ReadLine();
            var client = new XClient();
            client.OnPacketRecieve += OnPacketRecieve;
            client.Connect("127.0.0.1", 4910);

            // var rand = new Random();
            // _handshakeMagic = rand.Next();

            Thread.Sleep(1000);
            
            Console.WriteLine("Sending name packet..");

            client.QueuePacketSend(
                XPacketConverter.Serialize(
                    XPacketType.Name,
                    new XPacketName()
                    {
                        Name = name!
                    })
                    .ToPacket());

            while(true) {}
        }

        private static void OnPacketRecieve(byte[] packet)
        {
            var parsed = XPacket.Parse(packet);

            ProcessIncomingPacket(parsed);
        }

        private static void ProcessIncomingPacket(XPacket packet)
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
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ProcessHandshake(XPacket packet)
        {
            var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);

            if (_handshakeMagic - handshake.MagicHandshakeNumber == 15)
            {
                Console.WriteLine("Handshake successful!");
            }
        }
        
        private static void ProcessName(XPacket packet)
        {
            var name = XPacketConverter.Deserialize<XPacketName>(packet);

            Console.WriteLine($"Your Name is {name}");
        }
    }
}