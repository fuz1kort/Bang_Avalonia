using Avalonia.Media;


namespace Bang_Game.Models;

public class Player
{
    public byte Id { get; set; }
    public string? Name { get; set; }
    public string ColorString { get; set; }

    public Player(byte id,string name, uint rgb)
    {
        Id = id;
        Name = name;
        ColorString = Color.FromUInt32(rgb).ToString();
    }
    
    public void SetColor(uint rgb) => ColorString = Color.FromUInt32(rgb).ToString();
}