using Avalonia.Media.Imaging;

namespace Bang_Game.Models;

public class RuleImage
{
    public RuleImage(string ruleImagePath)
    {
        RuleImagePath = ruleImagePath;
        Bitmap = new Bitmap(RuleImagePath);
    }

    public string RuleImagePath { get; set; }

    public Bitmap Bitmap { get; set; }
}