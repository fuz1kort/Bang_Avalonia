using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketPlayers
{
    [XField(1)] public List<(string,uint)>? Players;

    public XPacketPlayers() {}

    public XPacketPlayers(List<(string,uint)>? players) => Players = players;
}