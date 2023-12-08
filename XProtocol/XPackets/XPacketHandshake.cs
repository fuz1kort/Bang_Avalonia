using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketHandshake
{
    [XField(1)]
    public int MagicHandshakeNumber;
}