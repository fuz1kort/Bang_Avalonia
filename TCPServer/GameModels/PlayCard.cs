namespace TCPServer.GameModels;

public class PlayCard: ICard
{
    public PlayCard(string? name, byte number, CardType cardType, PlayCardType playCardType, bool isConstant)
    {
        Name = name;
        Number = number;
        CardType = cardType;
        PlayCardType = playCardType;
        IsConstant = isConstant;
    }

    public string? Name { get; set; }
    public byte Number { get; set; }
    public CardType CardType { get; set; }
    public PlayCardType PlayCardType { get; set; }
    public bool IsConstant { get; set; }
}