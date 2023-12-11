using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketPlayers
{
    [XField(1)] public List<List<string>>? Players;

    public XPacketPlayers() {}

    public XPacketPlayers(List<List<string>>? players) => Players = players;
}