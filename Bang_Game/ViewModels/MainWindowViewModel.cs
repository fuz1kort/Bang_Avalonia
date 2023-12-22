using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Bang_Game_Models.Cards;
using Bang_Game.Models;
using ReactiveUI;

namespace Bang_Game.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private bool _isGoingToReset;
    private bool _canEndTurn;
    private bool _isChoosing;
    private byte _lastCard;
    public Player Player { get; }

    public ObservableCollection<Bitmap>? RuleImages { get; set; }

    public ReactiveCommand<Unit, Unit> ConnectCommand { get; }

    public ReactiveCommand<Unit, Unit> EndTurnCommand { get; }

    public ReactiveCommand<Unit, Unit> DropCardsToResetCommand { get; }

    public ReactiveCommand<byte, Unit> DropCardOnTableCommand { get; }

    public ReactiveCommand<byte, Unit> ChoosePlayerCommand { get; }

    public bool IsGoingToReset
    {
        get => _isGoingToReset;
        set
        {
            _isGoingToReset = value;
            OnPropertyChanged();
        }
    }

    public bool CanEndTurn
    {
        get => _canEndTurn;
        set
        {
            _canEndTurn = value;
            OnPropertyChanged();
        }
    }

    public bool IsChoosing
    {
        get => _isChoosing;
        set
        {
            _isChoosing = value;
            OnPropertyChanged();
        }
    }

    private bool _didShoot;

    public MainWindowViewModel()
    {
        Initialize();
        Player = new Player();
        Player.PropertyChanged += UpdateCanEndTurn;
        ConnectCommand = ReactiveCommand.Create(Connect);
        EndTurnCommand = ReactiveCommand.Create(EndTurn);
        DropCardOnTableCommand = ReactiveCommand.Create<byte>(DropCardOnTable);
        DropCardsToResetCommand = ReactiveCommand.Create(DropCardsToReset);
        ChoosePlayerCommand = ReactiveCommand.Create<byte>(DropCardOnTableToPlayer);
    }

    private void UpdateCanEndTurn(object? sender, PropertyChangedEventArgs e)
    {
        var player = (Player)sender!;
        var canEndTurn = player.CardsCount <= player.Hp && player.Turn;
        CanEndTurn = canEndTurn;
    }

    private void Initialize()
    {
        var files = Directory.GetFiles("../../../Assets/Rules/");
        var images = files.Select(x => new Bitmap(new FileInfo(x).FullName));
        RuleImages = new ObservableCollection<Bitmap>(images);
    }

    private void DropCardsToReset()
    {
        IsGoingToReset = !IsGoingToReset;
        var canEndTurn = Player.CardsCount <= Player.Hp && Player.Turn;
        CanEndTurn = canEndTurn;
    }

    private void DropCardOnTable(byte cardId)
    {
        if (!IsGoingToReset)
        {
            switch (Player.PlayCards[cardId].PlayCardType)
            {
                case PlayCardType.Bang:
                    if(_didShoot && Player.GunCard.PlayCardType != PlayCardType.Volcanic)
                        return;
                    IsChoosing = true;
                    _didShoot = true;
                    break;
                case PlayCardType.CatBalou:
                case PlayCardType.Panic:
                    IsChoosing = true;
                    break;
                default:
                    Player.DropCardOnTable(cardId);
                    break;
            }
        }
        else
            Player.DropCardToReset(cardId);

        _lastCard = cardId;
        var canEndTurn = Player.CardsCount <= Player.Hp && Player.Turn;
        CanEndTurn = canEndTurn;
    }

    private void DropCardOnTableToPlayer(byte playerId)
    {
        if (playerId == Player.Id || Player.PlayersList[playerId].Hp == 0) return;
        if(Player.PlayCards[_lastCard].PlayCardType != PlayCardType.CatBalou)
            if (Player.PlayersList[playerId].Distance > Player.ShotRange) return;
        
        Player.DropCardOnTable(_lastCard, playerId);
        IsChoosing = false;
        var canEndTurn = Player.CardsCount <= Player.Hp && Player.Turn;
        CanEndTurn = canEndTurn;
    }

    private void EndTurn()
    {
        Player.EndTurn();
        IsGoingToReset = false;
        CanEndTurn = false;
        _didShoot = false;
    }

    private void Connect() => Task.Run(() => Player.Connect());
}