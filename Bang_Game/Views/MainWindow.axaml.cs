using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Bang_Game.ViewModels;
using XProtocol;
using XProtocol.Serializer;
using XProtocol.XPackets;

namespace Bang_Game.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void CloseWindow_OnClick(object? sender, RoutedEventArgs e) => Close();

    private void OpenMainMenu_OnClick(object? sender, RoutedEventArgs e)
    {
        GreetingPage.IsVisible = false;
        MainMenuPage.IsVisible = true;
        RulesPage.IsVisible = false;
    }

    private void Rules_OnClick(object? sender, RoutedEventArgs e)
    {
        MainMenuPage.IsVisible = false;
        RulesPage.IsVisible = true;
    }

    private void StartButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainMenuPage.IsVisible = false;
        NickInput.IsVisible = true;
    }

    private void PlayerJoin_OnClick(object? sender, RoutedEventArgs e)
    {
        NickInput.IsVisible = false;
        var name = Nickname.Text;
        var client = new XClient();
        Task.Run(() => client.ConnectAsync(name!));
        client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.BeginPlayer, new XPacketBeginPlayer(name: name)).ToPacket());
        var gameWindow = new GameWindow { DataContext = new GameWindowViewModel() };
        gameWindow.Show();
        Close();
    }
}