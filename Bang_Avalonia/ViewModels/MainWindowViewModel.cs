using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;

namespace Bang_Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting => "Добро пожаловать в Бэнг!";
    public List<Bitmap> Rules { get; set; }

    public MainWindowViewModel()
    {
        Rules = new();
        Rules.AddRange(Directory.GetFiles("../../../Assets/Rules").Select(Image.FromFile).Select(x=> new Bitmap(x)).ToList());
    }

}