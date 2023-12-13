using TCPServer.GameModels;
using TCPServer.GameModels.Heroes;

namespace TCPServer.Services;

public class GeneratorService
{
    private Random _random = new();

    internal (List<RoleCard> roles, List<IHeroCard> heroes, Stack<ICard> deck) GenerateDecks()
    {
        var roles = new List<RoleCard>();
        var heroes = new List<IHeroCard>();
        var myList = new List<ICard>();
        var deck = new Stack<ICard>();

        //Добавлене оружия
        var volcanic1 = new GunCard(CardType.Clubs, "Вулканик", 10, 1);
        myList.Add(volcanic1);
        var volcanic2 = new GunCard(CardType.Spades, "Вулканик", 10, 1);
        myList.Add(volcanic2);
        var skofild1 = new GunCard(CardType.Clubs, "Скофилд", 11, 2);
        myList.Add(skofild1);
        var skofild2 = new GunCard(CardType.Clubs, "Скофилд", 12, 2);
        myList.Add(skofild2);
        var skofild3 = new GunCard(CardType.Spades, "Скофилд", 13, 2);
        myList.Add(skofild3);
        var remington = new GunCard(CardType.Clubs, "Ремингтон", 13, 3);
        myList.Add(remington);
        var karabin = new GunCard(CardType.Clubs, "Карабин", 14, 2);
        myList.Add(karabin);
        var vinchester = new GunCard(CardType.Spades, "Винчестер", 8, 2);
        myList.Add(vinchester);

        //Добавление других постоянных карт
        var mustang1 = new PlayCard("Мустанг", 8, CardType.Hearts,
            PlayCardType.Mustang, true);
        myList.Add(mustang1);
        var mustang2 = new PlayCard("Мустанг", 9, CardType.Hearts,
            PlayCardType.Mustang, true);
        myList.Add(mustang2);
        var scope = new PlayCard("Прицел", 14, CardType.Spades,
            PlayCardType.Scope, true);
        myList.Add(scope);
        var barrel1 = new PlayCard("Бочка", 12, CardType.Spades,
            PlayCardType.Barrel, true);
        myList.Add(barrel1);
        var barrel2 = new PlayCard("Бочка", 13, CardType.Spades,
            PlayCardType.Barrel, true);
        myList.Add(barrel2);

        //Добавление героев
        heroes.AddRange(new IHeroCard[]
        {
            new Billy(), new Eyes(), new James(), new Jane(), new Joe(), new Kit(), new Snake(), new Tom(), new Tyko()
        });

        //Добавлене разовых карт
        var wellsFargo = new PlayCard("Уэллс Фарго", 3, CardType.Hearts,
            PlayCardType.WellsFargo,
            false);
        myList.Add(wellsFargo);

        var stagecoach1 = new PlayCard("Дилижанс", 9, CardType.Spades,
            PlayCardType.Stagecoach,
            false);
        myList.Add(stagecoach1);
        myList.Add(stagecoach1);

        var gatling = new PlayCard("Гатлинг", 10, CardType.Hearts,
            PlayCardType.Gatling,
            false);
        myList.Add(gatling);

        var saloon = new PlayCard("Салун", 5, CardType.Hearts,
            PlayCardType.Saloon,
            false);
        myList.Add(saloon);

        for (var i = 9; i < 12; i++)
        {
            var catBalou = new PlayCard("Красотка", (byte)i, CardType.Diamonds,
                PlayCardType.CatBalou,
                false);
            myList.Add(catBalou);
        }

        var catBalou4 = new PlayCard("Красотка", 13, CardType.Hearts,
            PlayCardType.CatBalou,
            false);
        myList.Add(catBalou4);

        var panic1 = new PlayCard("Паника", 8, CardType.Diamonds,
            PlayCardType.Panic,
            false);
        myList.Add(panic1);
        var panic2 = new PlayCard("Паника", 11, CardType.Hearts,
            PlayCardType.Panic,
            false);
        myList.Add(panic2);
        var panic3 = new PlayCard("Паника", 12, CardType.Hearts,
            PlayCardType.Panic,
            false);
        myList.Add(panic3);
        var panic4 = new PlayCard("Паника", 14, CardType.Hearts,
            PlayCardType.Panic,
            false);
        myList.Add(panic4);

        for (var i = 6; i < 12; i++)
        {
            var beer = new PlayCard("Пиво", (byte)i, CardType.Hearts,
                PlayCardType.Beer,
                false);
            myList.Add(beer);
        }

        for (var i = 10; i < 15; i++)
        {
            var missed = new PlayCard("Мимо!", (byte)i, CardType.Clubs,
                PlayCardType.Missed,
                false);
            myList.Add(missed);
        }

        for (var i = 2; i < 9; i++)
        {
            var missed = new PlayCard("Мимо!", (byte)i, CardType.Spades,
                PlayCardType.Missed,
                false);
            myList.Add(missed);
        }

        var bang1 = new PlayCard("Бэнг!", 14, CardType.Spades,
            PlayCardType.Bang,
            false);
        myList.Add(bang1);

        for (var i = 2; i < 15; i++)
        {
            var bang = new PlayCard("Бэнг!", (byte)i, CardType.Diamonds,
                PlayCardType.Bang,
                false);
            myList.Add(bang);
        }

        for (var i = 2; i < 10; i++)
        {
            var bang = new PlayCard("Бэнг!", (byte)i, CardType.Clubs,
                PlayCardType.Bang,
                false);
            myList.Add(bang);
        }

        for (var i = 12; i < 15; i++)
        {
            var bang = new PlayCard("Бэнг!", (byte)i, CardType.Hearts,
                PlayCardType.Bang,
                false);
            myList.Add(bang);
        }

        var sheriff = new RoleCard(RoleType.Sheriff, true);
        roles.Add(sheriff);
        var bandit1 = new RoleCard(RoleType.Bandit, false);
        roles.Add(bandit1);
        var bandit2 = new RoleCard(RoleType.Bandit, false);
        roles.Add(bandit2);
        var renegade = new RoleCard(RoleType.Renegade, false);
        roles.Add(renegade);

        roles = roles.OrderBy(x => _random.Next()).ToList();
        heroes = heroes.OrderBy(x => _random.Next()).ToList();
        var cards = myList.OrderBy(x => _random.Next());
        foreach (var card in cards)
            deck.Push(card);

        return (roles, heroes, deck);
    }
}