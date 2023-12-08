using XProtocol.Serializer;

namespace XProtocol;

[Serializable]
public class XPacketName
{
    [XField(1)]
    public string Name;
}