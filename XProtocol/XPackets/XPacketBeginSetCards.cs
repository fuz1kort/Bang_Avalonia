using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketBeginSetCards
{
    [XField(1)] public List<byte>? Cards;
    
    public XPacketBeginSetCards()
    {
    }

    public XPacketBeginSetCards(List<byte> cards) => Cards = cards;
}