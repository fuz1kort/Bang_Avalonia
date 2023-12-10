using System.Collections.ObjectModel;
using Bang_Game.Models;

namespace Bang_Game.ViewModels;

public class GameWindowViewModel : ViewModelBase
{
    public static ObservableCollection<Player>? PlayersList { get; set; } = new(){new Player("Azamat", 000000)};
}