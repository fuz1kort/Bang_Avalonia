using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;

namespace Bang_Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting => "Добро пожаловать в Бэнг!";
    public List<Image> Rules { get; set; }

    public MainWindowViewModel()
    {
        Rules = new();
        Rules.AddRange(Directory.GetFiles("../../../Assets/Rules").Select(x=> Image.FromFile(x)));
    }

    public bool HandleButtonClick(int arg)
    {
        Console.WriteLine($"ButtonClicked {arg}");
        return true;
    }
    
    
}