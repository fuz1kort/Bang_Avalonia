namespace Bang_Game.Models.GameModels;

public interface ICard
{
    public string Name { get; set; }

    public byte Number { get; set; }

    public ECardType ECardType { get; set; }

    public bool IsConstant { get; set; }
}