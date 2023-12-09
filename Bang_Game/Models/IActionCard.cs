using Bang_Game.Models;

namespace Bang_Game.Models;

public interface IActionCard
{
    public string ActionName { get; set; }
    public byte Number { get; set; }
    public ECardType ECardType { get; set; }
}