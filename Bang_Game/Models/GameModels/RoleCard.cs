namespace Bang_Game.Models.GameModels;

public class RoleCard
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public ERoles ERole { get; set; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string RoleDescription { get; set; }
    
    public bool IsOpened { get; set; }

    public RoleCard(ERoles eRole, string roleDescription)
    {
        ERole = eRole;
        RoleDescription = roleDescription;
        IsOpened = eRole == ERoles.Sheriff;
    }
}