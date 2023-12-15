namespace Bang_Game.Models.Cards;

public class PlayCard
{
    public PlayCard(byte id,string? name, byte number, CardType cardType, PlayCardType playCardType, bool isConstant,
        byte shotRange = 0)
    {
        Id = id;
        Name = name;
        Number = number;
        CardType = cardType;
        PlayCardType = playCardType;
        IsConstant = isConstant;
        ShotRange = shotRange;
    }

    public PlayCard()
    {
    }

    public byte Id { get; }
    public string? Name { get; }
    public byte Number { get; }
    public CardType CardType { get; }
    public PlayCardType PlayCardType { get; }
    public bool IsConstant { get; }
    
    public byte ShotRange { get; set; }
}