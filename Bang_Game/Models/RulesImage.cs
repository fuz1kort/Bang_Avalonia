using Avalonia.Media.Imaging;

namespace Bang_Game.Models;

public class RulesImage
{
    public RulesImage(string ruleImagePath) => Bitmap = new Bitmap(ruleImagePath);
    public Bitmap Bitmap { get; set; }
}