
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Bang_Game.Models;
using Bang_Game.ViewModels;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace Bang_Game;

internal class XClient
{
    private readonly Queue<byte[]> _packetSendingQueue = new();

    private Socket? _socket;
    private IPEndPoint? _serverEndPoint;

    private Player Player { get; set; } = null!;

    public async Task ConnectAsync(string name)
    {
        try
        {
            ConnectAsync("127.0.0.1", 4910);



            QueuePacketSend(XPacketConverter.Serialize(XPacketType.Connection,
                new XPacketConnection
                {
                    IsSuccessfull = false
                }).ToPacket());

            Player = new Player(name, 0);

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
    

    public void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 256)
        {
            throw new Exception("Max packet size is 256 bytes.");
        }

        _packetSendingQueue.Enqueue(packet);
    }

    private async Task ReceivePacketsAsync()
    {
        while (true)
        {
            var buff = new byte[128];
            await _socket!.ReceiveAsync(buff);
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
                if (Player.Color == Color.FromArgb(0))
                {
                    ProcessBeginPlayer(packet);
                }
                else
                {
                    ReceivePlayer(packet);
                }
                    
                break;
            case XPacketType.Unknown:
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ReceivePlayer(XPacket packet)
    {
        var packetPlayer = XPacketConverter.Deserialize<XPacketBeginPlayer>(packet);
        GameWindowViewModel.PlayersList.Add(new Player(packetPlayer.Name!, packetPlayer.Argb));
    }

    private void ProcessConnection(XPacket packet)
    {
        var connection = XPacketConverter.Deserialize<XPacketConnection>(packet);

        if (connection.IsSuccessfull) Console.WriteLine("Handshake successful!");
    }

    private void ProcessBeginPlayer(XPacket packet)
    {
        var packetPlayer = XPacketConverter.Deserialize<XPacketBeginPlayer>(packet);
        Player.Name = packetPlayer.Name ?? Player.Name;
        
        var newColor = ColorTranslator.FromHtml(packetPlayer.Argb.ToString()!);
        Player.Color = newColor;
        
        Console.WriteLine($"Your Nickname is {Player.Name}");
        Console.WriteLine($"Your color is {Player.Color.Name}");
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
}