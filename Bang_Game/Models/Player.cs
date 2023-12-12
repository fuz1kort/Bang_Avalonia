using Avalonia.Media;


namespace Bang_Game.Models;

public class Player
{
    public string? Name { get; set; }
    public string ColorString { get; set; }

    public Player(string name, uint rgb)
    {
        Name = name;
        ColorString = Color.FromUInt32(rgb).ToString();
    }
    
    public void SetColor(uint rgb) => ColorString = Color.FromUInt32(rgb).ToString();
}