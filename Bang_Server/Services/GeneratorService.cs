namespace Bang_Server.Services;

public class GeneratorService
{
    private readonly Random _random = new();

    internal (Stack<byte> rolesDeck, Stack<string?> heroesDeck, Stack<byte> cardsDeck) GenerateDecks()
    {
        var roles = new List<byte> { 0, 1, 1, 2 };
        var heroes = new List<string?> { "МАЛЫШ БИЛЛИ", "ТУКО", "ТОМ КЕТЧУМ", "БОЛЬШОЙ ЗМЕЙ", "КИТ КАРСОН", "НЕУЛОВИМЫЙ ДЖО", "БЕДОВАЯ ДЖЕЙН", "ДЖЕССИ ДЖЕЙМС", "АНГЕЛЬСКИЕ ГЛАЗКИ", "ДЖАНГО", "ХЛАДНОКРОВНАЯ РОЗИ", "ЧЕЛОВЕК-БЕЗ-ИМЕНИ", "СЮЗИ ЛАФАЙЕТ", "БУТЧ КЭССИДИ", "БЕШЕНЫЙ ПЁС", "СЧАСТЛИВЧИК ЛЮК" };
        var cards = new List<byte>();
        for (byte id = 1; id < 68; id++) 
            cards.Add(id);
        
        var rolesDeck = new Stack<byte>();
        roles = roles.OrderBy(x => _random.Next()).ToList();
        foreach (var role in roles)
            rolesDeck.Push(role);
        
        var heroesDeck = new Stack<string?>();
        heroes = heroes.OrderBy(x => _random.Next()).ToList();
        foreach (var hero in heroes)
            heroesDeck.Push(hero);
        
        var cardsDeck = new Stack<byte>();
        cards = cards.OrderBy(x => _random.Next()).ToList();
        foreach (var card in cards)
            cardsDeck.Push(card);


        return (rolesDeck, heroesDeck, cardsDeck);
    }
    
    internal Stack<byte> GetNewDeck(IEnumerable<byte> reset)
    {
        var cards = reset.OrderBy(x => _random.Next()).ToList();
        var newDeck = new Stack<byte>();
        foreach (var card in cards) 
            newDeck.Push(card);
        return newDeck;
    }
}