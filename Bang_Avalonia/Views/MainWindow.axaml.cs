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
        Greeting.IsVisible = false;
        Continue.IsVisible = false;
        MainMenu.IsVisible = true;
    }

    private void Rules_OnClick(object? sender, RoutedEventArgs e)
    {
        MainMenu.IsVisible = false;
        Rules.IsVisible = true;
    }
}