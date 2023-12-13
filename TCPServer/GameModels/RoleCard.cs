namespace TCPServer.GameModels;

public class RoleCard
{
    public RoleCard(RoleType roleType, bool isOpened)
    {
        RoleType = roleType;
        IsOpened = isOpened;
    }

    public RoleType RoleType { get; set; }
    
    // public string? RoleDescription { get; set; }
    
    public bool IsOpened { get; set; }
}