using Discord;
using Discord.Commands;
using Rayiah.Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rayiah.Modules.Commands.Writing
{
    public class BoardsExperimental : Extended<SocketCommandContext>
    {
        [Command("Lol")]
        public async Task Lol()
        {
            string text = "~Yuna: Jeśli jeszcze widzisz mnie przytomną znaczy, że nic mi nie jest. Nie mogę się domyśleć, czy mówisz to ironicznie czy nie.Jesteś trudny do zrozumienia Kuai, bardziej niż matematyka..." +
                "Wypowiadając ostatnie zdanie, uniosła głowę i spojrzała na twarz chłopaka, żeby udowodnić, że nic jej nie jest. Lekko się uśmiechnęła i udali się do klasy." +
                "Po lekcjach znowu pozostali sami w klasie.Dzisiaj kończyli szybciej więc mieli czas aby uzupełnić swoje formularze zgłoszeniowe więc tym zajął się Kuai." +
                "Yuna podeszła do chłopaka, żeby sprawdzić jak radzi sobie z uzupełnianiem formularza." +
                "~Yuna: Cześć, wszystko idzie sprawnie? Jak skończysz daj mi znać, to pójdziemy razem i ich poznamy.Jeśli nie zgodzą się na kawę, pójdziesz ze mną tak czy siak.Muszę dzisiaj jakoś się utrzymać na nogach." +
                "Posłała mu sarkastyczny uśmieszek i wróciła na swoje miejsce.Tam rozpuściła swoje włosy, włożyła słuchawki i położyła się na ławce, twarzą do okna.Nie czuła się najlepiej, a muzyka jaka leciała jej w słuchawkach była na tyle smutna, że powoli jej oczy zaczęły się szklić." +
                "Kuai był zbyt skupiony na uzupełnianiu formularza aby ją usłyszeć. Gdy skończył, wstał i podszedł do Yuny oraz zdjął jej słuchawki." +
                "~Kuai: Hej, wszystko w porządku? Dzisiaj coś dziwnie się zachowujesz." +
                "Dziewczyna szybko otarła dłonią oczy, tak aby nikt nic nie zauważył." +
                "~Yuna: Gdyby to tylko dzisiaj..." +
                "Szepnęła pod nosem, podniosła się i spojrzała w górę na Kuaia. Jak siedziała, był jeszcze wyższy niż zazwyczaj i powoli jej kark miał tego dość.Usiadła więc na ławce i znowu spięła swoje włosy w typowy dla niej kucyk. " +
                "~Yuna: Jak mam być szczera, to nic nie jest w porządku ostatnio w moim życiu.Ale szkoła to nie miejsce na myślenie o tym i smutki, prawda?" +
                "Posłała mu lekki uśmiech i wyjęła dłoń tak, aby oddał jej słuchawki." +
                "Pomimo tego iż Yuna starała się zatrzeć śladu po płaczu, jej czerwone oczy jasno dały o tym znać Kuaiowi. Rozejrzał się po klasie i zauważył że nikogo w niej nie było." +
                "~Kuai: Rzadko staram się pomóc drugiej osobie, więc doceń to. Jak chcesz odebrać słuchawki powiedz mi co cię trapi." +
                "Kuai mówiąc te słowa usiadł na sąsiedniej ławce." +
                "~Yuna: Ehh, mam problemy jakby to powiedzieć, sama ze sobą.Nie wiem czy kiedykolwiek doświadczyłeś nienawiści do samego siebie, ale nienawiść w moim przypadku to zbyt delikatne słowo na to.Cieszę się, że chcesz mi pomóc, dużo to dla mnie znaczy, ale nie chce wyjść na desperatkę i mówić Ci czego w sobie nienawidzę.Brzmiałoby to jak prośba o komplementy i litość, a nie tego potrzebuję." +
                "Mówiąc to, jej głos się lekko załamywał, a przez cały ten czas patrzyła na swoje dłonie.";

            List<string> ToEmbed = new List<string>();
            do
            {
                ToEmbed.Add(text.Substring(0, 700) + "...");
                text = "..." + text.Substring(700);
            }
            while (text.Length > 700);
            ToEmbed.Add(text);

            PagedMessageCreator Creator = new PagedMessageCreator().AddInteractiveIEmotes(PagedPrefabs.BasicArrowModel());

            foreach (string t in ToEmbed)
            {
                Creator.AddPage(t);
            }

            await SendPagedMessage(Creator.Build());
        }

        [Command("BoardsSlideshow")]
        public async Task BoardsSlideShow()
        {
            try
            {
                var str = new DirectoryInfo($"./{Container.RES}/Characters/{Context.Guild.Id}")
                    .GetFiles()
                    .OrderBy(T => T.Name)
                    .ToArray();

                List<EmbedBuilder> Boards = new List<EmbedBuilder>();

                for (int x = 0; x < str.Length; x++)
                {
                    var (tr, y) = BoardManager.FormBoardBuilderEmbed(str[x].Name, Context);
                    if (tr)
                        Boards.Add(y);
                }

                PagedMessageCreator Creator = new PagedMessageCreator()
                    .AddPages(Boards.ToArray())
                    .AddInteractiveIEmotes(PagedPrefabs.BasicArrowModel());

                await SendPagedMessage(Creator.Build());
            }
            catch (Exception e) { Console.WriteLine(e); }
        }
    }
}