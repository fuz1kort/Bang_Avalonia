using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketName
{
    [XField(1)] public string? Name;
}