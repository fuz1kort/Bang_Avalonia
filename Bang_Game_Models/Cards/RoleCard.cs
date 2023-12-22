namespace Bang_Game_Models.Cards;

public class RoleCard
{
    public RoleCard(RoleType roleType, bool isOpened)
    {
        RoleType = roleType;
        IsOpened = isOpened;
    }

    public RoleType RoleType { get; }
    
    public bool IsOpened { get; set; }
}