using Bang_Game_Models.Cards;
using Bang_Game_Models.Cards.Heroes;

namespace Bang_Game_Models;

public static class CardsGenerator
{
    public static Dictionary<byte, PlayCard> GeneratePlayCards()
    {
        var playCards = new Dictionary<byte, PlayCard>();

        byte id = 1;
        var volcanic1 = new PlayCard(id, "ВУЛКАНИК", 10, CardType.Clubs, PlayCardType.Volcanic, 1);
        playCards[id++] = volcanic1;
        var volcanic2 = new PlayCard(id, "ВУЛКАНИК", 10, CardType.Spades, PlayCardType.Volcanic, 1);
        playCards[id++] = volcanic2;
        var schofield1 = new PlayCard(id, "СКОФИЛД", 11, CardType.Clubs, PlayCardType.Schofield, 2);
        playCards[id++] = schofield1;
        var schofield2 = new PlayCard(id, "СКОФИЛД", 12, CardType.Clubs, PlayCardType.Schofield, 2);
        playCards[id++] = schofield2;
        var schofield3 = new PlayCard(id, "СКОФИЛД", 13, CardType.Spades, PlayCardType.Schofield, 2);
        playCards[id++] = schofield3;
        var remington = new PlayCard(id, "РЕМИНГТОН", 13, CardType.Clubs, PlayCardType.Schofield, 2);
        playCards[id++] = remington;

        var mustang1 = new PlayCard(id, "МУСТАНГ", 8, CardType.Hearts,
            PlayCardType.Mustang);
        playCards[id++] = mustang1;
        var mustang2 = new PlayCard(id, "МУСТАНГ", 9, CardType.Hearts,
            PlayCardType.Mustang);
        playCards[id++] = mustang2;
        var scope = new PlayCard(id, "ПРИЦЕЛ", 14, CardType.Spades,
            PlayCardType.Scope);
        playCards[id++] = scope;
        var barrel1 = new PlayCard(id, "БОЧКА", 12, CardType.Spades,
            PlayCardType.Barrel);
        playCards[id++] = barrel1;
        var barrel2 = new PlayCard(id, "БОЧКА", 13, CardType.Spades,
            PlayCardType.Barrel);
        playCards[id++] = barrel2;

        var wellsFargo = new PlayCard(id, "УЭЛЛС ФАРГО", 3, CardType.Hearts,
            PlayCardType.WellsFargo);
        playCards[id++] = wellsFargo;

        var stagecoach1 = new PlayCard(id, "ДИЛИЖАНС", 9, CardType.Spades,
            PlayCardType.Stagecoach);
        playCards[id++] = stagecoach1;

        var stagecoach2 = new PlayCard(id, "ДИЛИЖАНС", 9, CardType.Spades,
            PlayCardType.Stagecoach);
        playCards[id++] = stagecoach2;

        var gatling = new PlayCard(id, "ГАТЛИНГ", 10, CardType.Hearts,
            PlayCardType.Gatling);
        playCards[id++] = gatling;

        var saloon = new PlayCard(id, "САЛУН", 5, CardType.Hearts,
            PlayCardType.Saloon);
        playCards[id++] = saloon;

        for (var i = 9; i < 12; i++)
        {
            var catBalou = new PlayCard(id, "КРАСОТКА", (byte)i, CardType.Diamonds,
                PlayCardType.CatBalou);
            playCards[id++] = catBalou;
        }

        var catBalou4 = new PlayCard(id, "КРАСОТКА", 13, CardType.Hearts,
            PlayCardType.CatBalou);
        playCards[id++] = catBalou4;

        var panic1 = new PlayCard(id, "ПАНИКА", 8, CardType.Diamonds,
            PlayCardType.Panic);
        playCards[id++] = panic1;
        var panic2 = new PlayCard(id, "ПАНИКА", 11, CardType.Hearts,
            PlayCardType.Panic);
        playCards[id++] = panic2;
        var panic3 = new PlayCard(id, "ПАНИКА", 12, CardType.Hearts,
            PlayCardType.Panic);
        playCards[id++] = panic3;
        var panic4 = new PlayCard(id, "ПАНИКА", 14, CardType.Hearts,
            PlayCardType.Panic);
        playCards[id++] = panic4;

        for (var i = 6; i < 12; i++)
        {
            var beer = new PlayCard(id, "ПИВО", (byte)i, CardType.Hearts,
                PlayCardType.Beer);
            playCards[id++] = beer;
        }

        for (var i = 10; i < 15; i++)
        {
            var missed = new PlayCard(id, "МИМО!", (byte)i, CardType.Clubs,
                PlayCardType.Missed);
            playCards[id++] = missed;
        }

        for (var i = 2; i < 9; i++)
        {
            var missed = new PlayCard(id, "МИМО!", (byte)i, CardType.Spades,
                PlayCardType.Missed);
            playCards[id++] = missed;
        }

        var bang1 = new PlayCard(id, "БЭНГ!", 14, CardType.Spades,
            PlayCardType.Bang);
        playCards[id++] = bang1;

        for (var i = 2; i < 15; i++)
        {
            var bang = new PlayCard(id, "БЭНГ!", (byte)i, CardType.Diamonds,
                PlayCardType.Bang);
            playCards[id++] = bang;
        }

        for (var i = 2; i < 10; i++)
        {
            var bang = new PlayCard(id, "БЭНГ!", (byte)i, CardType.Clubs,
                PlayCardType.Bang);
            playCards[id++] = bang;
        }

        for (var i = 12; i < 15; i++)
        {
            var bang = new PlayCard(id, "БЭНГ!", (byte)i, CardType.Hearts,
                PlayCardType.Bang);
            playCards[id++] = bang;
        }

        return playCards;
    }

    public static Dictionary<byte, RoleCard> GenerateRoleCards()
    {
        var roleCards = new Dictionary<byte, RoleCard>();

        var sheriff = new RoleCard(RoleType.Sheriff, true);
        roleCards[(byte)sheriff.RoleType] = sheriff;
        var bandit = new RoleCard(RoleType.Bandit, false);
        roleCards[(byte)bandit.RoleType] = bandit;
        var renegade = new RoleCard(RoleType.Renegade, false);
        roleCards[(byte)renegade.RoleType] = renegade;

        return roleCards;
    }

    public static Dictionary<string, HeroCard> GenerateHeroCards()
    {
        var heroCards = new Dictionary<string, HeroCard>();

        var billy = new Billy();
        heroCards[billy.HeroName!] = billy;
        var eyes = new Eyes();
        heroCards[eyes.HeroName!] = eyes;
        var james = new James();
        heroCards[james.HeroName!] = james;
        var jane = new Jane();
        heroCards[jane.HeroName!] = jane;
        var joe = new Joe();
        heroCards[joe.HeroName!] = joe;
        var kit = new Kit();
        heroCards[kit.HeroName!] = kit;
        var snake = new Snake();
        heroCards[snake.HeroName!] = snake;
        var tom = new Tom();
        heroCards[tom.HeroName!] = tom;
        var tuco = new Tuco();
        heroCards[tuco.HeroName!] = tuco;
        var butch = new Butch();
        heroCards[butch.HeroName!] = butch;
        var dog = new Dog();
        heroCards[dog.HeroName!] = dog;
        var jango = new Jango();
        heroCards[jango.HeroName!] = jango;
        var luke = new Luke();
        heroCards[luke.HeroName!] = luke;
        var noname = new Noname();
        heroCards[noname.HeroName!] = noname;
        var rosie = new Rosie();
        heroCards[rosie.HeroName!] = rosie;
        var susie = new Susie();
        heroCards[susie.HeroName!] = susie;

        return heroCards;
    }
}