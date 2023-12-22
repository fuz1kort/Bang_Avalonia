namespace XProtocol;

public static class XPacketTypeManager
{
    private static readonly Dictionary<XPacketType, Tuple<byte, byte>> TypeDictionary = new();

    static XPacketTypeManager()
    {
        RegisterType(XPacketType.Connection, 0, 0);
        RegisterType(XPacketType.UpdatedPlayerProperty, 1, 0);
        RegisterType(XPacketType.PlayersList, 2, 0);
        RegisterType(XPacketType.Turn, 3, 0);
        RegisterType(XPacketType.BangToPlayer, 3, 1);
        RegisterType(XPacketType.CardToTable, 4, 0);
        RegisterType(XPacketType.CardToReset, 4, 1);
        RegisterType(XPacketType.CardToPlayer, 4, 2);
        RegisterType(XPacketType.RemoveCard, 4, 3);
        RegisterType(XPacketType.Win, 5, 0);
        RegisterType(XPacketType.Lose, 5, 1);
    }

    private static void RegisterType(XPacketType type, byte btype, byte bsubtype)
    {
        if (TypeDictionary.ContainsKey(type))
            throw new Exception($"Packet type {type:G} is already registered.");

        TypeDictionary.Add(type, Tuple.Create(btype, bsubtype));
    }

    public static Tuple<byte, byte> GetType(XPacketType type)
    {
        if (!TypeDictionary.TryGetValue(type, out var value))
            throw new Exception($"Packet type {type:G} is not registered.");

        return value;
    }

    public static XPacketType GetTypeFromPacket(XPacket packet)
    {
        var type = packet.PacketType;
        var subtype = packet.PacketSubtype;

        foreach (var (xPacketType, tuple) in TypeDictionary)
        {
            if (tuple.Item1 == type && tuple.Item2 == subtype)
                return xPacketType;
        }

        return XPacketType.Unknown;
    }
}