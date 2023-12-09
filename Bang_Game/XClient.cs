using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Bang_Game.Models;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace Bang_Game;

internal class XClient
{
    private const int HandshakeMagic = 14;



    private readonly Queue<byte[]> _packetSendingQueue = new();

    private Socket? _socket;
    private IPEndPoint? _serverEndPoint;

    public void ConnectAsync(string ip, int port) => ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));

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
            case XPacketType.Handshake:
                ProcessHandshake(packet);
                break;
            case XPacketType.Name:
                ProcessName(packet);
                break;
            case XPacketType.Color:
                ProcessColor(packet);
                break;
            case XPacketType.Unknown:
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    private void ProcessColor(XPacket packet)
    {
        var colorPacket = XPacketConverter.Deserialize<XPacketColor>(packet);

        var color = Color.FromArgb(colorPacket.Argb);
        var newColor = ColorTranslator.FromHtml(colorPacket.Argb.ToString());
        Player.Instance.SetColor(color);
        
        Console.WriteLine($"Your color is {newColor.Name}");
    }

    private void ProcessHandshake(XPacket packet)
    {
        var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);

        if (handshake.MagicHandshakeNumber - HandshakeMagic == 10)
        {
            Console.WriteLine("Handshake successful!");
        }
    }

    private void ProcessName(XPacket packet)
    {
        var packetName = XPacketConverter.Deserialize<XPacketName>(packet);

        Console.WriteLine($"Your Nickname is {packetName.Name}");
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