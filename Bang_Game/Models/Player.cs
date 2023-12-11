using System.Drawing;

namespace Bang_Game.Models;

public class Player
{
    public string? Name { get; set; }
    public Color Color { get; set; }

    public Player(string name, int argb)
    {
        Name = name;
        Color = Color.FromArgb(argb);
    }

    public void SetColor(int argb) => Color = Color.FromArgb(argb);
}