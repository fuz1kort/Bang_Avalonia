using Bang_Cards_Models;
using XProtocol.Serializer;

namespace XProtocol.XPackets;

[Serializable]
public class XPacketBeginSet
{
    [XField(1)] public RoleCard? RoleCard;
    [XField(2)] public IHeroCard? HeroCard;
    [XField(3)] public List<ICard>? Cards;
    [XField(4)] public bool FirstTurn;
    
    public XPacketBeginSet(RoleCard? roleCard, IHeroCard? heroCard, List<ICard>? cards, bool firstTurn)
    {
        RoleCard = roleCard;
        HeroCard = heroCard;
        Cards = cards;
        FirstTurn = firstTurn;
    }
}