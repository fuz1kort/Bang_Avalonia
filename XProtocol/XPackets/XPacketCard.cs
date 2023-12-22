using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketCard
{
    [XField(1)] public byte CardId;
    [XField(2)] public byte ToPlayerId;

    public XPacketCard()
    {
    }

    public XPacketCard(byte cardId, byte toPlayerId = 10)
    {
        CardId = cardId;
        ToPlayerId = toPlayerId;
    }
}