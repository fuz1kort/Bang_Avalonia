using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Bang_Game.Models;
using ReactiveUI;

namespace Bang_Game.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly XClient _client;
    
    public ReactiveCommand<string,Unit> ConnectCommand { get; }

    public MainWindowViewModel()
    {
        Initialize();
        _client = new XClient();
        _client.PlayersReceivedEvent += Update;
        ConnectCommand = ReactiveCommand.Create<string>(Connect);
        _playersList = new ObservableCollection<Player>();
    }

    private void Connect(string name) => Task.Run(() => _client.ConnectAsync(name));

    private ObservableCollection<Player>? _playersList;
    private void Update(List<Player> players) => PlayersList.Add(players[PlayersList.Count]);

    public new event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ObservableCollection<Player>? PlayersList
    {
        get => _playersList;
        set
        {
            if (value == null) return;
            _playersList = value;
            OnPropertyChanged(nameof(PlayersList));

        }
    }

    private void Initialize()
    {
        var files = Directory.GetFiles("../../../Assets/Rules/");
        var images = files.Select(x => new RuleImage(new FileInfo(x).FullName));
        RuleImages = new ObservableCollection<RuleImage>(images);
        _playersList = new ObservableCollection<Player>();
    }

    public string Greeting => "Добро пожаловать в Бэнг!";

    private ObservableCollection<RuleImage>? _ruleImages;

    public ObservableCollection<RuleImage>? RuleImages
    {
        get => _ruleImages;
        set
        {
            if (value != null) _ruleImages = value;
        }
    }
}