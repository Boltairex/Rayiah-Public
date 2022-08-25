using Discord;
using Discord.Commands;
using Rayiah.Managers;
using Rayiah.Objects.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Rayiah.Tools.Utilites;

namespace Rayiah.Modules.Commands.Writing
{
    [Help("UnitBoards", "Komendy przeznaczone do tworzenia boardów jednostek. Przeznaczony do opisania grup, czy zbiorów żołnierzy." +
    "\nPrzykładowe wykorzystanie komendy: >UCreateBoard Synth.", 5)]
    public class UnitBoards : ModuleBase<SocketCommandContext>
    {
        public static bool UReType = false;

        [Command("UCreateBoard"), Alias("UCB"), HelpMethod("Tworzy jednostkę o konkretnej nazwie.", true)]
        public async Task CreateBoard([Remainder] string character)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            if (Container.BannedCharsFilter(character))
            {
                await TempMessage(this, $"Użyto niedozwolonych znaków. Nie używaj w nazwie:\n `{Container.GetBannedCharacters()}`", 5000);
                return;
            }

            if (character.Contains('"'))
                await TempMessage(this, $"Użyto niedozwolonych znaków. Nie używaj w nazwie:\n `{Container.GetBannedCharacters()}`", 5000);

            var board = new UnitBoard()
            {
                name = character,
                guildID = Context.Guild.Id,
            };

            if (!Directory.Exists($"./{Paths.RES}/Units/{Context.Guild.Id}"))
                Directory.CreateDirectory($"./{Paths.RES}/Units/{Context.Guild.Id}");

            if (!Boards.CheckExistance<UnitBoard>(character, Context.Guild.Id))
            {
                Boards.SaveBoard(board, Context.Guild.Id);
                await Context.Channel.SendMessageAsync($"Utworzono jednostkę pod ID: {character}");
            }
            else
                await Context.Channel.SendMessageAsync($"Taka jednostka już istnieje, jeżeli jej nie chcesz, wpisz: >URemoveBoard *ID*, gdzie *ID* to nazwa jednostki.");
        }

        [Command("UGetBoardJSON"), Alias("UGBJ"), HelpMethod("Wyciąga raw-JSON z jednostki.")]
        public async Task GetBoardJSON([Remainder] string fileID)
        {
            var s = Boards.GetJSONFile<UnitBoard>(fileID, Context.Guild.Id);
            if (s == "None")
                await TempMessage(this, "Nie ma takiej jednostki.");
            else
                await Context.Channel.SendMessageAsync($"```json\n{s}```");
        }

        [Command("USetBoardJSON"), Alias("USBJ"), HelpMethod("Wgrywa tekst w formie JSON'a do jednostki. Jeżeli nie umiesz JSON'a, nie próbuj tego robić - grozi korupcją boarda.", true)]
        public async Task SetBoardJSON(string fileID, [Remainder] string content)
        {
            var s = Boards.LoadJSONFile<UnitBoard>(fileID, Context.Guild.Id, content);
            if (s)
                await TempMessage(this, "Załadowano.");
            else
                await TempMessage(this, "Nie ma takiej jednostki.");

        }

        [Command("USetDesc"), HelpMethod("Zmienia opis jednostek.", true)]
        public async Task CommandDesc(string fileID, [Remainder] string desc)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
            if (get)
            {
                ch.desc = desc;
                Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage(this, $"Nie ma takiej jednostki.");
        }

        [Command("USetColor"), HelpMethod("Ustawia kolor jednostki.", true)]
        public async Task ChangeBoardColor(string fileID, [Remainder] string Color)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
            if (get)
            {
                ch.color = Color;
                Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage(this, $"Nie ma takiej jednostki.");
        }

        [Command("USetName"), HelpMethod("Zmienia nazwę jednostki.", true)]
        public async Task CommandName(string fileID, [Remainder] string newName)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            if (Container.BannedCharsFilter(newName))
            {
                await TempMessage(this, $"Użyto niedozwolonych znaków. Nie używaj w nazwie:\n `{Container.GetBannedCharacters()}`", 5000);
                return;
            }

            var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
            if (get)
            {
                if (!Boards.CheckExistance<UnitBoard>(newName, Context.Guild.Id))
                {
                    ch.name = newName;
                    Boards.SaveBoard(ch, Context.Guild.Id);
                    ch.name = fileID;
                    Boards.RequestDelete(ch, Context.Guild.Id);
                    await TempMessage(this, $"Zmieniono.");
                    await Context.Message.DeleteAsync();
                }
                else
                    await TempMessage(this, $"Istnieje jednostka o takiej nazwie.");
            }
            else
                await TempMessage(this, $"Nie ma takiej jednostki.");
        }

        [Command("USetURL"), HelpMethod("Zmienia obrazek jednostki.", true)]
        public async Task CommandURL(string fileID, [Remainder] string URL)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }


            var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
            if (get)
            {
                ch.url = URL;
                Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage(this, $"Nie ma takiej jednostki.");
        }

        [Command("USetWeaponry")]
        public async Task CommandClothes(string fileID, [Remainder] string weaponry)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
            if (get)
            {
                ch.weaponry = weaponry;
                Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage(this, $"Nie ma takiej jednostki.");
        }

        [Command("USetPower")]
        public async Task CommandPower(string fileID, uint power)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
            if (get)
            {
                ch.power = Convert.ToInt32(power);
                Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage(this, $"Nie ma takiej jednostki.");
        }

        [Command("UShowFields")]
        public async Task ShowFields(string fileID)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
            if (get)
            {
                EmbedBuilder embed = new EmbedBuilder();
                for (int x = 0; x < ch.fields.Length; x++)
                    embed.AddField($"{x}", $"{ch.fields[x].name}, sortowane: {ch.fields[x].sorted}");

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
                await TempMessage(this, $"Nie ma takiej jednostki.");
        }

        [Command("UDeleteField")]
        public async Task Deletefield(string fileID, string name)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
            if (get)
            {
                var list = ch.fields.ToList();
                for (int x = 0; x < ch.fields.Length; x++)
                {
                    if (list[x].name == name)
                        list.RemoveAt(x);
                }
                ch.fields = list.ToArray();
                Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage(this, $"Nie ma takiej jednostki.");
        }

        [Command("UAddField")]
        public async Task AddField(string fileID, string name, string desc, bool sorted = false)
        {
            try
            {
                if (!Container.instance.CheckUserAuthority(Context.User))
                {
                    await TempMessage(this, $"Nie masz permisji.");
                    return;
                }

                var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
                if (get)
                {
                    UCustomField field = new UCustomField()
                    {
                        name = name,
                        desc = desc,
                        sorted = sorted
                    };

                    if (ch.fields != null)
                        ch.fields = ch.fields.Append(field).ToArray();
                    else
                        ch.fields = new UCustomField[1] { field };

                    Boards.SaveBoard(ch, Context.Guild.Id);
                    await TempMessage(this, $"Zmieniono.");
                    await Context.Message.DeleteAsync();
                }
                else
                    await TempMessage(this, $"Nie ma takiej jednostki.");
            }
            catch (Exception E) { Console.WriteLine(E); }
        }

        [Command("UChangeField")]
        public async Task ChangeField(string fileID, string name, string desc)
        {
            try
            {
                if (!Container.instance.CheckUserAuthority(Context.User))
                {
                    await TempMessage(this, $"Nie masz permisji.");
                    return;
                }

                var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
                if (get)
                {
                    for (int x = 0; x < ch.fields.Length; x++)
                    {
                        if (ch.fields[x].name == name)
                        {
                            UCustomField newField = new UCustomField()
                            {
                                name = name,
                                desc = desc,
                                sorted = ch.fields[x].sorted
                            };
                            ch.fields[x] = newField;
                            break;
                        }
                    }

                    Boards.SaveBoard(ch, Context.Guild.Id);
                    await TempMessage(this, $"Zmieniono.");
                    await Context.Message.DeleteAsync();
                }
                else
                    await TempMessage(this, $"Nie ma takiej jednostki.");
            }
            catch (Exception E) { Console.WriteLine(E); }
        }

        [Command("UBoard")]
        public async Task ShowBoard([Remainder] string fileID)
        {
            try
            {
                var (get, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
                if (get)
                {
                    var embed = new EmbedBuilder();

                    if (Boards.VerifyValue(ch.desc, out string descVal))
                        embed.AddField("Opis", descVal);

                    if (Boards.VerifyValue(ch.weaponry, out string weaponVal))
                        embed.AddField("Wyposażenie", weaponVal);

                    embed.WithAuthor(Context.User)
                        .WithColor(ch.color == null ? Color.LightOrange : GetColor(ch.color))
                        .WithTitle($"{ch.name} - {ch.power}");

                    if (ch.id != null)
                        embed.WithDescription($"*Identyfikator: {ch.id}*");

                    if (ch.url != null)
                        embed.WithImageUrl(ch.url);

                    if (ch.fields != null)
                    {
                        for (int x = 0; x < ch.fields.Length; x++)
                        {
                            ch.fields[x] = Boards.VerifyField(ch.fields[x]);
                            embed.AddField(ch.fields[x].name, ch.fields[x].desc, ch.fields[x].sorted);
                        }
                    }
                    await Context.Channel.SendMessageAsync("", false, embed.Build());

                }
                else
                    await TempMessage(this, $"Nie ma takiej jednostki.");
            }
            catch (Exception E) { Console.WriteLine(E); }
        }

        [Command("USetID")]
        public async Task SetUnitID(string fileID, string id)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            var (check, ch) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
            if (check)
            {
                ch.id = id;
                Boards.SaveBoard(ch, Context.Guild.Id);
                await TempMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await TempMessage(this, $"Nie ma takiej jednostki, bądź komenda jest już w użyciu.");
        }

        [Command("URemoveBoard")]
        public async Task CommandClothes([Remainder] string fileID)
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            var (check, board) = Boards.ReadBoard<UnitBoard>(fileID, Context.Guild.Id);
            if (check && !UReType)
            {
                await TempMessage(this, $"Masz 4 sekundy na usunięcie jednostki: {fileID}. Użyj komendy >UAccept", 4500);
                if (UReType)
                {
                    Boards.RequestDelete(board, Context.Guild.Id);
                    await TempMessage(this, $"Usunięto {board.name}", 1000);
                    UReType = false;
                }
                else
                    await TempMessage(this, "Odrzucono", 1000);
            }
            else
                await TempMessage(this, $"Nie ma takiej jednostki, bądź komenda jest już w użyciu.");
        }

        [Command("UAccept")]
        public async Task AcceptDelete()
        {
            if (!Container.instance.CheckUserAuthority(Context.User))
            {
                await TempMessage(this, $"Nie masz permisji.");
                return;
            }

            UReType = true;
            await DeleteMessage(this);
        }

        [Command("URanking")]
        public async Task ShowRanking()
        {
            var str = new DirectoryInfo($"./{Paths.RES}/Units/{Context.Guild.Id}")
                .GetFiles()
                .ToArray();

            List<UnitBoard> boards = new List<UnitBoard>();

            foreach (FileInfo F in str)
            {
                var (get, board) = Managers.Boards.ReadBoard<UnitBoard>(F.Name.Split('.')[0], Context.Guild.Id);
                if (get)
                    boards.Add(board);
            }

            boards = boards.OrderBy(f => f.power).ToList();
            boards.Reverse();

            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.User)
                .WithTitle("RANKING")
                .WithColor(Color.DarkRed)
                .WithDescription("Ranking jednostek po posiadanej mocy");

            if (boards.Count == 0)
                embed.AddField("Brak!", "Póki co nie ma nikogo w rankingu.");
            else
            {
                if (boards.Count > 0)
                {
                    string id = "";
                    if (boards[0].id != null)
                        id = $" {boards[0].id},";

                    embed.AddField($" [-1-]{id} {boards[0].name} z mocą {boards[0].power}!", "*PIERWSZE MIEJSCE!*", false);
                    embed.WithThumbnailUrl(boards[0].url);
                    if (boards.Count > 1)
                    {
                        id = "";
                        if (boards[1].id != null)
                            id = $" {boards[1].id},";

                        embed.AddField($" [-2-]{id} {boards[1].name} z mocą {boards[1].power}!", "*DRUGIE MIEJSCE!*", false);
                        if (boards.Count > 2)
                        {
                            id = "";
                            if (boards[2].id != null)
                                id = $" {boards[2].id},";

                            embed.AddField($" [-3-]{id} {boards[2].name} z mocą {boards[2].power}!", "*TRZECIE MIEJSCE!*", false);
                            if (boards.Count > 3)
                            {
                                string s = "```rust\n";
                                for (int x = 3; x < boards.Count; x++)
                                {
                                    id = "";
                                    if (boards[x].id != null)
                                        id = $" {boards[x].id},";

                                    s += $"{x + 1}{id} {boards[x].name} z mocą {boards[x].power}.\n";
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

        [Command("UAllBoards")]
        public async Task ShowBoards()
        {
            try
            {
                var str = new DirectoryInfo($"./{Paths.RES}/Units/{Context.Guild.Id}")
                    .GetFiles()
                    .OrderBy(T => T.Name)
                    .ToArray();

                float strCount = 0;
                string output = "";

                for (int x = 0; x < str.Length; x++)
                {
                    string id = "";
                    var (get, board) = Boards.ReadBoard<UnitBoard>(str[x].Name, Context.Guild.Id);

                    if (get && board.id != null)
                        id = $" *{board.id}* -";

                    output += $"**{x + 1}**.{id} {str[x].Name.Split(".json")[0]}\n";
                    strCount += str[x].Name.Length;
                    if (strCount > 1700)
                    {
                        strCount = 0;
                        await Context.Channel.SendMessageAsync(output);
                        output = "";
                    }
                }
                if (string.IsNullOrWhiteSpace(output))
                    await Context.Channel.SendMessageAsync("Nie ma żadnych jednostek.");
                else
                    await Context.Channel.SendMessageAsync(output);
            }
            catch (Exception E) { Console.WriteLine(E); }
        }
    }
}