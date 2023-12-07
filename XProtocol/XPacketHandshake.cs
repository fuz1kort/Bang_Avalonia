using XProtocol.Serializer;

namespace XProtocol;

public class XPacketHandshake
{
    [XField(1)]
    public int MagicHandshakeNumber;
}