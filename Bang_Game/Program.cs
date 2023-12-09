using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Threading;
using System.Threading.Tasks;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace Bang_Game;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // BackConsoleAsync();
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static async Task BackConsoleAsync()
    {
        try
        {
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    
}