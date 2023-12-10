using Avalonia.Media.Imaging;

namespace Bang_Game.Models;

public class RuleImage
{
    public RuleImage(string ruleImagePath) => Bitmap = new Bitmap(ruleImagePath);
    public Bitmap Bitmap { get; set; }
}