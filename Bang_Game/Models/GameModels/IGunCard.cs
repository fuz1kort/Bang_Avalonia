namespace Bang_Game.Models.GameModels;

public interface IGunCard: ICard
{
    public byte ShotRange { get; set; }
}