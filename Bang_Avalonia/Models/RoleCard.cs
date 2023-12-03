namespace Bang_Avalonia.Models;

public class RoleCard
{
    public Roles Role { get; set; }
    public string RoleDescription { get; set; }
    
    public RoleCard(Roles role, string roleDescription)
    {
        Role = role;
        RoleDescription = roleDescription;
    }
}