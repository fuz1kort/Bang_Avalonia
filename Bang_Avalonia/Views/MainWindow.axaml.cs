using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Bang_Avalonia.Views;

public partial class MainWindow : Window
{

    public MainWindow() => InitializeComponent();

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

    private void CloseWindow_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void StartButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainMenuPage.IsVisible = false;
    }
}