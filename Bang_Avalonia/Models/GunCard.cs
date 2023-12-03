namespace Bang_Avalonia.Models;

public class GunCard
{
    public string GunName { get; set; }
    public byte ShotRange { get; set; }
    public byte Number { get; set; }
    public CardType CardType { get; set; }
    
    public GunCard(string gunName, byte shotRange, byte number, CardType cardType)
    {
        GunName = gunName;
        ShotRange = shotRange;
        Number = number;
        CardType = cardType;
    }
}