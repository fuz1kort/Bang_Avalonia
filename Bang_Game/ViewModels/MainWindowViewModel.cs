using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Bang_Game.Models;
using ReactiveUI;

namespace Bang_Game.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public Player Player { get; }

    public ReactiveCommand<Unit, Unit> ConnectCommand { get; }

    public ReactiveCommand<Unit, Unit> EndTurnCommand { get; }

    public MainWindowViewModel()
    {
        Initialize();
        Player = new Player();
        ConnectCommand = ReactiveCommand.Create(Connect);
        EndTurnCommand = ReactiveCommand.Create(EndTurn);
    }

    private void EndTurn() => Player.EndTurn();

    private void Connect() => Task.Run(() => Player.Connect());

    private void Initialize()
    {
        var files = Directory.GetFiles("../../../Assets/Rules/");
        var images = files.Select(x => new RulesImage(new FileInfo(x).FullName));
        RuleImages = new ObservableCollection<RulesImage>(images);
    }

    public ObservableCollection<RulesImage>? RuleImages { get; set; }
}