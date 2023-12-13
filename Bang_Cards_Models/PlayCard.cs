namespace Bang_Cards_Models;

public class PlayCard
{
    public PlayCard(string? name, byte number, CardType cardType, PlayCardType playCardType, bool isConstant,
        byte shotRange = 0)
    {
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

    public string? Name { get; }
    public byte Number { get; }
    public CardType CardType { get; }
    public PlayCardType PlayCardType { get; }
    public bool IsConstant { get; }
    
    public byte ShotRange { get; set; }
}