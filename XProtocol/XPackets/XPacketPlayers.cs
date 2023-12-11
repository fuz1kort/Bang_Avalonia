using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketPlayers
{
    [XField(1)] public List<(string, int)>? Players;

    public XPacketPlayers() {}

    public XPacketPlayers(List<(string, int)>? players) => Players = players;
}