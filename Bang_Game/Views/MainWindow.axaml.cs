using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

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
        GameGrid.IsVisible = true;
        Background = Brushes.White;
    }
}