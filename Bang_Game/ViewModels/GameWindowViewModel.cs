using System.Collections.Generic;
using Bang_Game.Models;

namespace Bang_Game.ViewModels;

public class GameWindowViewModel : ViewModelBase
{
    public static List<Player>? PlayersList { get; set; }
}