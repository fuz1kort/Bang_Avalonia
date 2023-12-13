using System.Collections.Generic;
using Avalonia.Media;
using Bang_Cards_Models;


namespace Bang_Game.Models;

public class Player
{
    public byte Id { get; set; }
    
    public string? Name { get; set; }
    
    public string ColorString { get; set; }
    
    public RoleCard? Role { get; set; }
    
    public IHeroCard? Hero { get; set; }
    
    public List<ICard>? Cards { get; set; }
    
    private bool Turn { get; set; }

    public Player(byte id,string name, uint rgb)
    {
        Id = id;
        Name = name;
        ColorString = Color.FromUInt32(rgb).ToString();
    }
    
    public void SetColor(uint rgb) => ColorString = Color.FromUInt32(rgb).ToString();

    public void BeginSet(RoleCard role, IHeroCard hero, List<ICard> cards, bool firstTurn)
    {
        Role = role;
        Hero = hero;
        Cards = cards;
        Turn = firstTurn;
    }
}