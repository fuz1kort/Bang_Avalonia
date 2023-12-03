namespace Bang_Avalonia.Models;

public class HeroCard
{
    public string HeroName { get; set; }
    public string HeroDescription { get; set; }
    public byte HeroHp { get; set; }
    
    public HeroCard(string heroName, string heroDescription, byte heroHp)
    {
        HeroName = heroName;
        HeroDescription = heroDescription;
        HeroHp = heroHp;
    }
}