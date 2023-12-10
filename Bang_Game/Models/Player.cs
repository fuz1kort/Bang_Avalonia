using Avalonia.Media;

namespace Bang_Game.Models;

public class Player
{
    public string? Name { get; set; }
    public Brush Color { get; set; }

    public Player(string name, uint argb)
    {
        Name = name;
        Color = new SolidColorBrush(argb);
    }

    public void SetColor(uint argb) => Color = new SolidColorBrush(argb);
}