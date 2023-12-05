using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Bang_Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
    }

    private void OpenMainMenu(object? sender, RoutedEventArgs e)
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

    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}