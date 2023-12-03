namespace Bang_Avalonia.Models;

public class ActionCard
{
    public string ActionName { get; set; }
    public byte Number { get; set; }
    public CardType CardType { get; set; }
    
    public ActionCard(string actionName, byte number, CardType cardType)
    {
        ActionName = actionName;
        Number = number;
        CardType = cardType;
    }
}