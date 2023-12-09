namespace Bang_Game.Models.Heroes;

public class Tyko: IHeroCard
{
    public string HeroName { get; set; } = "Туко";
    // public string HeroDescription { get; set; } = "В фазе набора может взять первую карту с верха сброса";
    public byte HeroHp { get; set; } = 4;
}