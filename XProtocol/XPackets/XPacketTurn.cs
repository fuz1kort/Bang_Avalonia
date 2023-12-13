using XProtocol.Serializer;

namespace XProtocol.XPackets;

public class XPacketTurn
{
    [XField(1)] public bool Turn;
}