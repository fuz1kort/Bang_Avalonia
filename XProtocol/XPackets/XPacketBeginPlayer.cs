using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketBeginPlayer
{
    [XField(1)] public string? Name;
    [XField(2)] public int Argb;

    public XPacketBeginPlayer() {}

    public XPacketBeginPlayer(string? name) => Name = name;

    public XPacketBeginPlayer(int argb) : this() => Argb = argb;
}