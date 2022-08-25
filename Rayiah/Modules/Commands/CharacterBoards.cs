using Discord;
using Discord.Commands;
using Rayiah.Managers;
using Rayiah.Objects.Attributes;
using Rayiah.Objects.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rayiah.Modules.Commands
{
    [Help("CBoards", "Komendy przeznaczone do tworzenia boardów postaci." +
        "\nPrzykładowe wykorzystanie komendy: >CCreateBoard Boltu.", 5)]
    public class CharacterBoards : RayiahModule<SocketCommandContext>
    {
        public static bool reType = false;

        public Dictionary<IUser, CharacterBoardData> toDelete = new Dictionary<IUser, CharacterBoardData>();

        [Command("CCreateBoard"), Alias("CCB"), Info("Tworzy postać o konkretnej nazwie.", true)]
        public async Task CreateBoard([Remainder] string character)
        {
            if (Container.BannedCharsFilter(character))
            {
                await TempMessage($"Użyto niedozwolonych znaków. Nie używaj w nazwie:\n `{Container.GetBannedCharacters()}`", 5000);
                return;
            }

            var board = new CharacterBoardData()
            {
                name = character,
                guildID = Context.Guild.Id,
                showRanking = true,
            };

            if (!Directory.Exists($"./{Paths.RES}/Characters/{Context.Guild.Id}"))
                Directory.CreateDirectory($"./{Paths.RES}/Characters/{Context.Guild.Id}");

            if (!Managers.Boards.CheckExistance<CharacterBoardData>(character, Context.Guild.Id))
            {
                Managers.Boards.SaveBoard(board, Context.Guild.Id);
                var x = await Context.Channel.SendMessageAsync($"Utworzono postać pod ID: {character}");
            }
            else
                await Context.Channel.SendMessageAsync($"Taka postać już istnieje, jeżeli jej nie chcesz, wpisz: >RemoveBoard *ID*, gdzie *ID* to nazwa postaci.");
        }

        [Command("CBoard"), Info("Odczytuje postać z bazy.", true)]
        public async Task ShowBoard([Remainder] string fileID)
        {
            var (get, emb) = Managers.Boards.FormEmbedBoard(fileID, Context);
            if (get)
                await Context.Channel.SendMessageAsync("", false, emb);
            else
                await TempMessage($"Nie ma takiej postaci.");
        }

        [Command("CGetBoardJSON"), Alias("CGBJ"), Info("Wyciąga raw-JSON z postaci.")]
        public async Task GetBoardJSON([Remainder] string fileID)
        {
            var s = Managers.Boards.GetJSONFile<CharacterBoardData>(fileID, Context.Guild.Id);
            if (s == "None")
                await TempMessage("Nie ma takiej postaci.");
            else
                await Context.Channel.SendMessageAsync($"```json\n{s}```");
        }
        //Jeżeli nie umiesz JSON'a, nie używaj go.
        [Command("CSetBoardJSON"), Alias("CSBJ"), Info("Wgrywa tekst w formie JSON'a do postaci. Jeżeli nie umiesz JSON'a, nie próbuj tego robić - grozi korupcją postaci.", true)]
        public async Task SetBoardJSON(string fileID, [Remainder] string content)
        {
            var s = Managers.Boards.LoadJSONFile<CharacterBoardData>(fileID, Context.Guild.Id, content);
            if (s)
                await TempMessage("Załadowano.");
            else
                await TempMessage("Nie ma takiego Boarda.");

        }

        [Command("CSetDesc"), Info("Zmienia opis postaci.", true)]
        public async Task CommandDesc(string fileID, [Remainder] string desc)
        {
            var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id, true);
            if (get)
            {
                ch.desc = desc;
                Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage($"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage($"Nie ma takiej postaci.");
        }

        [Command("CSetAge"), Info("Zmienia wiek postaci.", true)]
        public async Task CommandAge(string fileID, [Remainder] string age)
        {
            var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id, true);
            if (get)
            {
                ch.age = age;
                Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage($"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage($"Nie ma takiej postaci.");
        }

        [Command("CSetRanking"), Info("Ustawia, czy postać powinna podlegać rankingowi.", true)]
        public async Task CommandDesc(string fileID, bool ranking)
        {
            var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id, true);
            if (get)
            {
                ch.showRanking = ranking;
                Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage($"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage($"Nie ma takiej postaci.");
        }

        [Command("CSetColor"), Info("Ustawia kolor dla postaci. Kolory można znaleźć pod >Colors.")]
        public async Task ChangeBoardColor(string fileID, [Remainder] string Color)
        {
            var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id, true);
            if (get)
            {
                ch.color = Color;
                Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage($"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage($"Nie ma takiej postaci.");
        }

        [Command("CSetName"), Info("Zmienia nazwę postaci.", true)]
        public async Task CommandName(string fileID, [Remainder] string newName)
        {
            if (Container.BannedCharsFilter(newName))
            {
                await TempMessage($"Użyto niedozwolonych znaków. Nie używaj w nazwie:\n `{Container.GetBannedCharacters()}`", 5000);
                return;
            }

            var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id, true);
            if (get)
            {
                if (!Managers.Boards.CheckExistance<CharacterBoardData>(newName, Context.Guild.Id))
                {
                    ch.name = newName;
                    Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                    ch.name = fileID;
                    Managers.Boards.RequestDelete(ch, Context.Guild.Id);
                    await TempMessage($"Zmieniono.");
                    await Context.Message.DeleteAsync();
                }
                else
                    await TempMessage($"Istnieje postać o takim nicku.");
            }
            else
                await TempMessage($"Nie ma takiej postaci.");
        }

        [Command("CSetURL"), Info("Zmienia obrazek postaci.", true)]
        public async Task CommandURL(string fileID, [Remainder] string URL)
        {
            var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id, true);
            if (get)
            {
                ch.url = URL;
                Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage($"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage($"Nie ma takiej postaci.");
        }

        [Command("CSetAppearance"), Alias("CSApp"), Info("Zmienia wygląd postaci.", true)]
        public async Task CommandClothes(string fileID, [Remainder] string clothes)
        {
            var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id, true);
            if (get)
            {
                ch.clothes = clothes;
                Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage($"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage($"Nie ma takiej postaci.");
        }

        [Command("CSetPower"), Info("Zmienia siłę postaci.", true)]
        public async Task CommandPower(string fileID, uint power)
        {
            var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id, true);
            if (get)
            {
                ch.power = Convert.ToInt32(power);
                Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage($"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage($"Nie ma takiej postaci.");
        }

        [Command("CShowFields"), Alias("CSF"), Info("Pokazuje wszystkie dostępne 'CustomField'y w postaci.", true)]
        public async Task ShowFields(string fileID)
        {
            var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id);
            if (get)
            {
                EmbedBuilder embed = new EmbedBuilder();
                for (int x = 0; x < ch.fields.Length; x++)
                    embed.AddField($"{x}", $"{ch.fields[x].name}, sortowane: {ch.fields[x].sorted}");

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
                await TempMessage($"Nie ma takiej jednostki.");
        }

        [Command("CDeleteField"), Alias("CDF"), Info("Usuwa z postaci konkretnego fielda. Wymaga podania nazwy postaci, a potem fielda.", true)]
        public async Task Deletefield(string fileID, string name)
        {
            var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id);
            if (get)
            {
                var list = ch.fields.ToList();
                for (int x = 0; x < ch.fields.Length; x++)
                {
                    if (list[x].name == name)
                        list.RemoveAt(x);
                }
                ch.fields = list.ToArray();
                Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage($"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage($"Nie ma takiej jednostki.");
        }


        [Command("CAddField"), Info("Dodaje 'CustomField'a do postaci według schematu: nazwa, nazwaFielda, opis, czySortowane.", true)]
        public async Task AddField(string fileID, string name, string desc, bool sorted = false)
        {
            try
            {
                var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id);
                if (get)
                {
                    EmbedCustomFieldData field = new EmbedCustomFieldData()
                    {
                        name = name,
                        desc = desc,
                        sorted = sorted
                    };

                    if (ch.fields != null)
                        ch.fields = ch.fields.Append(field).ToArray();
                    else
                        ch.fields = new EmbedCustomFieldData[1] { field };

                    Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                    await TempMessage($"Zmieniono.");
                    await Context.Message.DeleteAsync();
                }
                else
                    await TempMessage($"Nie ma takiej jednostki.");
            }
            catch (Exception E) { Console.WriteLine(E); }
        }

        [Command("CChangeField"), Info("Zmienia konkretny field postaci (nie zmienia kolejności wyświetlania).", true)]
        public async Task ChangeField(string fileID, string name, string desc)
        {
            try
            {
                var (get, ch) = Managers.Boards.ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id);
                if (get)
                {
                    for (int x = 0; x < ch.fields.Length; x++)
                    {
                        if (ch.fields[x].name == name)
                        {
                            EmbedCustomFieldData newField = new EmbedCustomFieldData()
                            {
                                name = name,
                                desc = desc,
                                sorted = ch.fields[x].sorted
                            };

                            ch.fields[x] = newField;
                            break;
                        }
                    }

                    Managers.Boards.SaveBoard(ch, Context.Guild.Id);
                    await TempMessage($"Zmieniono.");
                    await Context.Message.DeleteAsync();
                }
                else
                    await TempMessage($"Nie ma takiej jednostki.");
            }
            catch (Exception E) { Console.WriteLine(E); }
        }

        [Command("CRemoveBoard"), Alias("CRB"), Info("Tworzy próbę usunięcia postaci. Wymaga potwierdzenia", true)]
        public async Task CommandClothes([Remainder] string character)
        {
            var (check, board) = Managers.Boards.ReadBoard<CharacterBoardData>(character, Context.Guild.Id);
            if (check && !reType)
            {
                await TempMessage($"Masz 4 sekundy na usunięcie postaci: {character}. Użyj komendy >ChAccept", 4500);
                if(toDelete.ContainsKey(Context.User))
                {
                    Boards.RequestDelete(board, Context.Guild.Id);
                    await TempMessage($"Usunięto {board.name}", 1000);

                    toDelete.Remove(Context.User);
                }
                else
                    await TempMessage("Odrzucono", 1000);
            }
            else
                await TempMessage($"Nie ma takiej postaci, bądź komenda jest już w użyciu.");
        }

        [Command("CDeleteAccept"), Alias("CDA"), Info("Potwierdza próbę usunięcia postaci", true)]
        public async Task AcceptDelete()
        {
            reType = true;
            await DeleteMessage();
        }

        [Command("CRanking"), Alias("CR"), Info("Wyświetla ranking postaci.")]
        public async Task ShowRanking()
        {
            var str = new DirectoryInfo($"./{Paths.RES}/Characters/{Context.Guild.Id}")
                .GetFiles()
                .ToArray();

            List<CharacterBoardData> boards = new List<CharacterBoardData>();

            foreach (FileInfo F in str)
            {
                try
                {
                    var (get, board) = Managers.Boards.ReadBoard<CharacterBoardData>(F.Name.Split('.')[0], Context.Guild.Id);
                    if (get)
                    {
                        if (board.showRanking)
                            boards.Add(board);
                    }
                }
                catch { }
            }

            boards = boards.OrderBy(f => f.power).ToList();
            boards.Reverse();

            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.User)
                .WithTitle("RANKING")
                .WithColor(Color.DarkRed)
                .WithDescription("Ranking postaci po posiadanej mocy");

            if (boards.Count == 0)
                embed.AddField("Brak!", "Póki co nie ma nikogo w rankingu.");
            else
            {
                if (boards.Count > 0)
                {
                    embed.AddField($" [-1-] {boards[0].name} z mocą {boards[0].power}!", "*PIERWSZE MIEJSCE!*", false);
                    embed.WithThumbnailUrl(boards[0].url);
                    if (boards.Count > 1)
                    {
                        embed.AddField($" [-2-] {boards[1].name} z mocą {boards[1].power}!", "*DRUGIE MIEJSCE!*", false);
                        if (boards.Count > 2)
                        {
                            embed.AddField($" [-3-] {boards[2].name} z mocą {boards[2].power}!", "*TRZECIE MIEJSCE!*", false);
                            if (boards.Count > 3)
                            {
                                string s = "```rust\n";
                                for (int x = 3; x < boards.Count; x++)
                                {
                                    s += $"{x + 1}. {boards[x].name} z mocą {boards[x].power}.\n";
                                    if (x == 25)
                                    {
                                        s += "I reszta...";
                                        break;
                                    }
                                }
                                s += "```";
                                embed.AddField("Reszta:", s, false);
                            }
                        }
                    }
                }
            }
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("CAllBoards"), Info("Wyświetla wszystkie postacie zawarte w bazie danych (z serwera).")]
        public async Task ShowBoards()
        {
            var str = new DirectoryInfo($"./{Paths.RES}/Characters/{Context.Guild.Id}")
                .GetFiles()
                .OrderBy(T => T.Name)
                .ToArray();

            float strCount = 0;
            string output = "";

            for (int x = 0; x < str.Length; x++)
            {
                output += $"{x + 1}. {str[x].Name.Split(".json")[0]}\n";
                strCount += str[x].Name.Length;
                if (strCount > 1700)
                {
                    strCount = 0;
                    await Context.Channel.SendMessageAsync(output);
                    output = "";
                }
            }
            if (string.IsNullOrWhiteSpace(output))
                await Context.Channel.SendMessageAsync("Nie ma żadnych postaci.");
            else
                await Context.Channel.SendMessageAsync(output);
        }
    }
}