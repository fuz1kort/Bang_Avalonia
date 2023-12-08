using XProtocol.Serializer;

namespace XProtocol;

public struct XPacketName
{
    [XField(1)]
    public string Name;
}