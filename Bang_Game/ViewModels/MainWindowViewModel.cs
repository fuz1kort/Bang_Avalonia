using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Bang_Avalonia.Models;

namespace Bang_Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel() => Initialize();

    private void Initialize()
    {
        var files = Directory.GetFiles("../../../Assets/Rules/");
        var images = files.Select(x => new RuleImage(new FileInfo(x).FullName));
        RuleImages = new ObservableCollection<RuleImage>(images);
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