namespace Bang_Game.Models.Cards;

public class RoleCard
{
    public RoleCard(RoleType roleType, bool isOpened)
    {
        RoleType = roleType;
        IsOpened = isOpened;
    }

    public RoleType RoleType { get; }
    
    // public string? RoleDescription { get; set; }
    
    public bool IsOpened { get; set; }
}