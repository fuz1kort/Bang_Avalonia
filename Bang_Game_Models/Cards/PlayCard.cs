namespace Bang_Game_Models.Cards;

public class PlayCard
{
    public PlayCard(byte id,string? name, byte number, CardType cardType, PlayCardType playCardType, byte shotRange = 0)
    {
        Id = id;
        Name = name;
        Number = number;
        CardType = cardType;
        PlayCardType = playCardType;
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
    
    public byte ShotRange { get; set; }
}