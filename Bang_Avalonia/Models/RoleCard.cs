namespace Bang_Avalonia.Models;

public class RoleCard
{
    public ERoles ERole { get; set; }
    public string RoleDescription { get; set; }
    
    public RoleCard(ERoles eRole, string roleDescription)
    {
        ERole = eRole;
        RoleDescription = roleDescription;
    }
}