namespace TCPServer.Services;

public class GeneratorService
{
    private readonly Random _random = new();

    internal (Stack<byte> rolesDeck, Stack<string?> heroesDeck, Stack<byte> cardsDeck) GenerateDecks()
    {
        var roles = new List<byte> { 0, 1, 1, 2 };
        var heroes = new List<string?> { "Малыш Билли", "Туко", "Том Кетчум", "Большой Змей", "Кит Карсон", "Неуловимый Джо", "Бедовая Джейн", "Джесси Джеймс", "Ангельские Глазки", "Джанго", "Хладнокровная Рози", "Человек-без-имени", "Сюзи Лафайет", "Бутч Кэссиди", "Бешеный Пёс", "Счастливчик Люк" };
        var cards = new List<byte>();
        for (byte id = 1; id < 67; id++) 
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
}