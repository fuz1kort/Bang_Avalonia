namespace Bang_Cards_Models;

public class GunCard: ICard
{
    public GunCard(CardType cardType, string? name, byte number, byte shotRange)
    {
        CardType = cardType;
        Name = name;
        Number = number;
        ShotRange = shotRange;
    }

    public string? Name { get; set; }
    public byte Number { get; set; }
    public CardType CardType { get; set; }
    public bool IsConstant { get; set; } = true;
    public byte ShotRange { get; set; }
}