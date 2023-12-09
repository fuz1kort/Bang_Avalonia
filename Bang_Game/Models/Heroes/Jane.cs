namespace Bang_Game.Models.Heroes;

public class Jane: IHeroCard
{
    public string HeroName { get; set; } = "Бедовая Джейн";
    // public string HeroDescription { get; set; } = "Может играть карты \"Бэнг!\" как карты \"Мимо!\" и наоборот";
    public byte HeroHp { get; set; } = 4;
}