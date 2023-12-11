using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bang_Game.Models;

namespace Bang_Game.ViewModels;

public class GameWindowViewModel : ViewModelBase
{
    public GameWindowViewModel()
    {
        PlayersList = new ObservableCollection<Player>();
    }
    
    private static ObservableCollection<Player>? _playerList;

    public static void SetPlayersList(List<Player> players)
    {
        for (var i = 0; i < players.Count; i++)
        {
            if (players.Count - _playerList!.Count >= 1)
                _playerList.Add(players[i]);
            else
                _playerList[i] = players[i];
        }
    }
    public ObservableCollection<Player>? PlayersList
    {
        get => _playerList;
        set
        {
            if (value != null) _playerList = value;
        }
    }
}