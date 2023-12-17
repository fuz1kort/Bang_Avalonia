using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketBytesList
{
    [XField(1)] public List<byte>? BytesList;

    public XPacketBytesList()
    {
    }

    public XPacketBytesList(List<byte> bytesList) => BytesList = bytesList;
}