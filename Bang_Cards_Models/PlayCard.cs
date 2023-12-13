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

    public string? Name { get; set; }
    public byte Number { get; set; }
    public CardType CardType { get; set; }
    public PlayCardType PlayCardType { get; set; }
    public bool IsConstant { get; set; }
    
    public byte ShotRange { get; set; }
}