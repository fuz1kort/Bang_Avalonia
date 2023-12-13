namespace TCPServer.GameModels;

public interface ICard
{
    public string? Name { get; set; }

    public byte Number { get; set; }

    public CardType CardType { get; set; }

    public bool IsConstant { get; set; }
}