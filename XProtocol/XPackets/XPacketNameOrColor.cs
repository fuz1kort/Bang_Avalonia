using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketNameOrColor
{
    [XField(1)] public string? NameOrColor;
    
    public XPacketNameOrColor() {}

    public XPacketNameOrColor(string nameOrColor) => NameOrColor = nameOrColor;
}