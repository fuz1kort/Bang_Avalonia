﻿namespace TCPServer.GameModels;

public interface IHeroCard
{
    public string HeroName { get; set; }
    
    public byte HeroHp { get; set; }
}