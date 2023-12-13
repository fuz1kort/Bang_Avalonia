using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketPlayers
{
    [XField(1)] public List<(byte,string,uint)>? Players;

    public XPacketPlayers() {}

    public XPacketPlayers(List<(byte,string,uint)>? players) => Players = players;
}