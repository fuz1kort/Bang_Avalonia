using Bang_Cards_Models;
using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketBeginSet
{
    [XField(1)] public List<PlayCard>? Cards12;
    
    [XField(2)] public List<PlayCard>? Cards34;
    
    [XField(3)] public PlayCard? Card5;
    
    [XField(4)] public RoleCard? RoleCard;
    
    [XField(5)] public HeroCard? HeroCard;
  

    public XPacketBeginSet() { }
    
    public XPacketBeginSet(RoleCard roleCard, HeroCard heroCard, List<PlayCard> cards)
    {
        RoleCard = roleCard;
        HeroCard = heroCard;
        Cards12 = new List<PlayCard>
        {
            cards[0],
            cards[1]
        };

        Cards34 = new List<PlayCard> { cards[2] };
        if(cards.Count > 3)
            Cards34.Add(cards[3]);
        Card5 = cards.Count > 4 ? cards[4] : new PlayCard();
    }
}