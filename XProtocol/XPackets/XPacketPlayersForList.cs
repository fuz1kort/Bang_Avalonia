using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketPlayersForList
{
    [XField(1)] public List<(byte, string, string)>? Players;
}