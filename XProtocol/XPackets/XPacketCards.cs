using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketCards
{
    [XField(1)] public List<byte>? Cards;

    public XPacketCards()
    {
    }

    public XPacketCards(List<byte> cards) => Cards = cards;
}