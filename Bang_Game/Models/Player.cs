using System.Drawing;

namespace Bang_Game.Models;

public class Player
{
    public string? Name { get; set; }
    public Color Color { get; set; }

    public Player(string name) => Name = name;
}