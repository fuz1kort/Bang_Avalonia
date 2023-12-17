using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketId
{
    [XField(1)] public byte Id;

    public XPacketId()
    {
    }

    public XPacketId(byte id) => Id = id;
}