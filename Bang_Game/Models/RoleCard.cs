using Bang_Game.Models;

namespace Bang_Game.Models;

public class RoleCard
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public ERoles ERole { get; set; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string RoleDescription { get; set; }

    public RoleCard(ERoles eRole, string roleDescription)
    {
        ERole = eRole;
        RoleDescription = roleDescription;
    }
}