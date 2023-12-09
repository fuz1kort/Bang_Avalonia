﻿using XProtocol;

namespace XProtocol;

public static class XPacketTypeManager
{
    private static readonly Dictionary<XPacketType, Tuple<byte, byte>> TypeDictionary = new();

    static XPacketTypeManager()
    {
        RegisterType(XPacketType.Handshake, 1, 0);
        RegisterType(XPacketType.Name, 2, 0);
    }

    private static void RegisterType(XPacketType type, byte btype, byte bsubtype)
    {
        if (TypeDictionary.ContainsKey(type))
        {
            throw new Exception($"Packet type {type:G} is already registered.");
        }

        TypeDictionary.Add(type, Tuple.Create(btype, bsubtype));
    }

    public static Tuple<byte, byte> GetType(XPacketType type)
    {
        if (!TypeDictionary.TryGetValue(type, out var value))
        {
            throw new Exception($"Packet type {type:G} is not registered.");
        }

        return value;
    }

    public static XPacketType GetTypeFromPacket(XPacket packet)
    {
        var type = packet.PacketType;
        var subtype = packet.PacketSubtype;

        foreach (var (xPacketType, tuple) in TypeDictionary)
        {
            if (tuple.Item1 == type && tuple.Item2 == subtype)
            {
                return xPacketType;
            }
        }

        return XPacketType.Unknown;
    }
}