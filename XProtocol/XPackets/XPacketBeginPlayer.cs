using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketBeginPlayer
{
    [XField(1)] public string? Name;
    [XField(2)] public uint Argb;

    public XPacketBeginPlayer() {}

    public XPacketBeginPlayer(string? name) => Name = name;

    public XPacketBeginPlayer(uint argb) : this() => Argb = argb;
}