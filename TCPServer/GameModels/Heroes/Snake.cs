﻿namespace TCPServer.GameModels.Heroes;

public class Snake: IHeroCard
{
    public string HeroName { get; set; } = "Большой Змей";
    public byte HeroHp { get; set; } = 4;
}