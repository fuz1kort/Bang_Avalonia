using Bang_Cards_Models;
using Bang_Cards_Models.Heroes;

namespace TCPServer.Services;

public class GeneratorService
{
    private Random _random = new();

    internal (Stack<RoleCard> roles, Stack<IHeroCard> heroes, Stack<ICard> deck) GenerateDecks()
    {
        var cards = new List<ICard>();
        var roles = new List<RoleCard>();
        var heroes = new List<IHeroCard>();

        //Добавлене оружия
        var volcanic1 = new GunCard(CardType.Clubs, "Вулканик", 10, 1);
        cards.Add(volcanic1);
        var volcanic2 = new GunCard(CardType.Spades, "Вулканик", 10, 1);
        cards.Add(volcanic2);
        var skofild1 = new GunCard(CardType.Clubs, "Скофилд", 11, 2);
        cards.Add(skofild1);
        var skofild2 = new GunCard(CardType.Clubs, "Скофилд", 12, 2);
        cards.Add(skofild2);
        var skofild3 = new GunCard(CardType.Spades, "Скофилд", 13, 2);
        cards.Add(skofild3);
        var remington = new GunCard(CardType.Clubs, "Ремингтон", 13, 3);
        cards.Add(remington);
        var karabin = new GunCard(CardType.Clubs, "Карабин", 14, 2);
        cards.Add(karabin);
        var vinchester = new GunCard(CardType.Spades, "Винчестер", 8, 2);
        cards.Add(vinchester);

        //Добавление других постоянных карт
        var mustang1 = new PlayCard("Мустанг", 8, CardType.Hearts,
            PlayCardType.Mustang, true);
        cards.Add(mustang1);
        var mustang2 = new PlayCard("Мустанг", 9, CardType.Hearts,
            PlayCardType.Mustang, true);
        cards.Add(mustang2);
        var scope = new PlayCard("Прицел", 14, CardType.Spades,
            PlayCardType.Scope, true);
        cards.Add(scope);
        var barrel1 = new PlayCard("Бочка", 12, CardType.Spades,
            PlayCardType.Barrel, true);
        cards.Add(barrel1);
        var barrel2 = new PlayCard("Бочка", 13, CardType.Spades,
            PlayCardType.Barrel, true);
        cards.Add(barrel2);

        //Добавление героев
        heroes.AddRange(new IHeroCard[]
        {
            new Billy(), new Eyes(), new James(), new Jane(), new Joe(), new Kit(), new Snake(), new Tom(), new Tyko()
        });

        //Добавлене разовых карт
        var wellsFargo = new PlayCard("Уэллс Фарго", 3, CardType.Hearts,
            PlayCardType.WellsFargo,
            false);
        cards.Add(wellsFargo);

        var stagecoach1 = new PlayCard("Дилижанс", 9, CardType.Spades,
            PlayCardType.Stagecoach,
            false);
        cards.Add(stagecoach1);
        cards.Add(stagecoach1);

        var gatling = new PlayCard("Гатлинг", 10, CardType.Hearts,
            PlayCardType.Gatling,
            false);
        cards.Add(gatling);

        var saloon = new PlayCard("Салун", 5, CardType.Hearts,
            PlayCardType.Saloon,
            false);
        cards.Add(saloon);

        for (var i = 9; i < 12; i++)
        {
            var catBalou = new PlayCard("Красотка", (byte)i, CardType.Diamonds,
                PlayCardType.CatBalou,
                false);
            cards.Add(catBalou);
        }

        var catBalou4 = new PlayCard("Красотка", 13, CardType.Hearts,
            PlayCardType.CatBalou,
            false);
        cards.Add(catBalou4);

        var panic1 = new PlayCard("Паника", 8, CardType.Diamonds,
            PlayCardType.Panic,
            false);
        cards.Add(panic1);
        var panic2 = new PlayCard("Паника", 11, CardType.Hearts,
            PlayCardType.Panic,
            false);
        cards.Add(panic2);
        var panic3 = new PlayCard("Паника", 12, CardType.Hearts,
            PlayCardType.Panic,
            false);
        cards.Add(panic3);
        var panic4 = new PlayCard("Паника", 14, CardType.Hearts,
            PlayCardType.Panic,
            false);
        cards.Add(panic4);

        for (var i = 6; i < 12; i++)
        {
            var beer = new PlayCard("Пиво", (byte)i, CardType.Hearts,
                PlayCardType.Beer,
                false);
            cards.Add(beer);
        }

        for (var i = 10; i < 15; i++)
        {
            var missed = new PlayCard("Мимо!", (byte)i, CardType.Clubs,
                PlayCardType.Missed,
                false);
            cards.Add(missed);
        }

        for (var i = 2; i < 9; i++)
        {
            var missed = new PlayCard("Мимо!", (byte)i, CardType.Spades,
                PlayCardType.Missed,
                false);
            cards.Add(missed);
        }

        var bang1 = new PlayCard("Бэнг!", 14, CardType.Spades,
            PlayCardType.Bang,
            false);
        cards.Add(bang1);

        for (var i = 2; i < 15; i++)
        {
            var bang = new PlayCard("Бэнг!", (byte)i, CardType.Diamonds,
                PlayCardType.Bang,
                false);
            cards.Add(bang);
        }

        for (var i = 2; i < 10; i++)
        {
            var bang = new PlayCard("Бэнг!", (byte)i, CardType.Clubs,
                PlayCardType.Bang,
                false);
            cards.Add(bang);
        }

        for (var i = 12; i < 15; i++)
        {
            var bang = new PlayCard("Бэнг!", (byte)i, CardType.Hearts,
                PlayCardType.Bang,
                false);
            cards.Add(bang);
        }

        var sheriff = new RoleCard(RoleType.Sheriff, true);
        roles.Add(sheriff);
        var bandit1 = new RoleCard(RoleType.Bandit, false);
        roles.Add(bandit1);
        var bandit2 = new RoleCard(RoleType.Bandit, false);
        roles.Add(bandit2);
        var renegade = new RoleCard(RoleType.Renegade, false);
        roles.Add(renegade);
        
        var cardsDeck = new Stack<ICard>();
        cards = cards.OrderBy(x => _random.Next()).ToList();
        foreach (var card in cards)
            cardsDeck.Push(card);
        
        var rolesDeck = new Stack<RoleCard>();
        roles = roles.OrderBy(x => _random.Next()).ToList();
        foreach (var role in roles)
            rolesDeck.Push(role);
        
        var heroesDeck = new Stack<IHeroCard>();
        heroes = heroes.OrderBy(x => _random.Next()).ToList();
        foreach (var hero in heroes)
            heroesDeck.Push(hero);
        


        return (rolesDeck, heroesDeck, cardsDeck);
    }
}