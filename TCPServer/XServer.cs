using System.Drawing;
using System.Net;
using System.Net.Sockets;

namespace TCPServer;

internal class XServer
{
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    // ReSharper disable once CollectionNeverQueried.Local
    private List<ConnectedClient> _clients = new();

    internal static List<Color> Colors = new() { Color.Red, Color.Yellow, Color.Green, Color.Blue };

    private bool _lobbyfull;
    private bool _listening;
    private bool _stopListening;

    public Task StartAsync()
    {
        try
        {
            if (_listening)
            {
                throw new Exception("Server is already listening incoming requests.");
            }

            _socket.Bind(new IPEndPoint(IPAddress.Any, 4910));
            _socket.Listen(10);

            _listening = true;

            Console.WriteLine("Server have been started");
            var stopThread = new Thread(() =>
            {
                while (_listening)
                    if (Console.ReadLine() == "stop")
                        Stop();
            });
            stopThread.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Task.CompletedTask;
    }

    private void Stop()
    {
        if (!_listening)
            throw new Exception("Server is already not listening incoming requests.");
        _stopListening = true;
        _listening = false;
        _socket.Close();
        Console.WriteLine("Server have been stopped");
    }

    public void AcceptClients()
    {
        while (!_lobbyfull)
        {
            if (_stopListening)
                return;

            Socket client;

            try
            {
                client = _socket.Accept();
            }
            catch
            {
                return;
            }

            Console.WriteLine($"[!] Accepted client from {(IPEndPoint)client.RemoteEndPoint!}");

            var c = new ConnectedClient(client);
            _clients.Add(c);
            if (_clients.Count == 4)
                _lobbyfull = true;
        }

        while (_clients.Count == 4)
        {
            
        }
        
        
    }
}