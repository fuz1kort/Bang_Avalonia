namespace Bang_Avalonia.Models;

public interface IGunCard
{
    public string GunName { get; set; }
    public byte ShotRange { get; set; }
    public byte Number { get; set; }
    public ECardType ECardType { get; set; }
}