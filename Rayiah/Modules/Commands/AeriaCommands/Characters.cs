using Discord;
using Discord.Commands;
using Rayiah.AeriaStories;
using Rayiah.AeriaStories.Objects;
using Rayiah.Management;
using System;
using System.Threading.Tasks;
using static Rayiah.Management.Container;
using static Rayiah.Tools.RayiahUtilites;

namespace Rayiah.Modules.AeriaCommands
{
    public class Characters : ModuleBase<SocketCommandContext>
    {
        [Command(">CreateCharacter")]//CreateItem "" "" "" 0
        public async Task CreateCharacter(string name)
        {
            if (Container.instance.BannedCharsFilter(name))
                await CreateMessage(this,$"Użyto niedozwolonych znaków: \n`{getBannedCharactersInString}`",5000);
            else
            {
                if (CharacterSerializer.CheckExistance(Context.User.Id.ToString()))
                    await CreateMessage(this, "Masz już utworzoną postać.");
                else
                {
                    var board = new CharacterInstance()
                    {
                        name = name
                    };
                    CharacterSerializer.SaveBoard(board, Context.User.Id);

                    await CreateMessage(this, "Utworzono postać. Zapoznaj się z komendami dzięki >>Aeria.");
                }
            }
        }

        [Command(">RequestDelete")]
        public async Task DeleteCharacter(ulong u)
        {
            if (!instance.CheckUserAuthority(Context.User))
            {
                await CreateMessage(this, "Brak permisji.");
                return;

                if (CharacterSerializer.RequestDelete(u))
                    await CreateMessage(this, "Usunięto postać.", 3000);
                else
                    await CreateMessage(this, "Nie ma takiej postaci.", 3000);
            }
        }

        [Command(">RequestDelete")]
        public async Task DeleteCharacter()
        {
            if (!CharacterSerializer.ToDelete.Contains(Context.User.Id))
                CharacterSerializer.ToDelete.Add(Context.User.Id);
            else
            {
                await CreateMessage(this, "Postać jest gotowa do usunięcia, potwierdź usunięcie komendą >>ConfirmDelete.", 4000);
                return;
            }
            await CreateMessage(this, "Przygotowanie do usunięcia. Jeżeli jesteś na sto procent pewien, by usunąć cały swój postęp, wpisz >>ConfirmDelete.", 6000);
        }
        
        [Command(">ConfirmDelete")]
        public async Task ConfirmDelete() => Init.ConfirmDeleteV(Context.User.Id, Context);
        

        [Command(">GetBoardJSON")]
        public async Task GetBoardJSON()
        {
            var s = CharacterSerializer.GetJSONFile(Context.User.Id.ToString());
            if (s == "None")
                await CreateMessage(this, "Nie ma takiej postaci.");
            else
                await Context.Channel.SendMessageAsync($"```json\n{s}```");
        }

        [Command(">SetAge")]
        public async Task CommandAge([Remainder] string age)
        {
            if (age.Length > 20)
            {
                await CreateMessage(this, "Wiek nie może przekraczać 20 liter długości.");
                return;
            }

            var (get, ch) = CharacterSerializer.ReadBoard(Context.User.Id.ToString());
            if (get)
            {
                ch.age = age;
                CharacterSerializer.SaveBoard(ch, Context.User.Id);
                await CreateMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await CreateMessage(this, $"Nie ma takiej postaci.");
        }

        [Command(">SetDesc")]
        public async Task CommandDesc([Remainder] string desc)
        {
            var (get, ch) = CharacterSerializer.ReadBoard(Context.User.Id.ToString());
            if (get)
            {
                ch.desc = desc;
                CharacterSerializer.SaveBoard(ch, Context.User.Id);
                await CreateMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await CreateMessage(this, $"Nie ma takiej postaci.");
        }

        [Command(">SetURL")]
        public async Task CommandUrl([Remainder] string url)
        {
            var (get, ch) = CharacterSerializer.ReadBoard(Context.User.Id.ToString());
            if (get)
            {
                ch.url = url;
                CharacterSerializer.SaveBoard(ch, Context.User.Id);
                await CreateMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await CreateMessage(this, $"Nie ma takiej postaci.");
        }

        [Command(">SetColor")]
        public async Task CommandColor([Remainder] string color)
        {
            var (get, ch) = CharacterSerializer.ReadBoard(Context.User.Id.ToString());
            if (get)
            {
                ch.color = color;
                CharacterSerializer.SaveBoard(ch, Context.User.Id);
                await CreateMessage(this, $"Zmieniono.");
                await Context.Message.DeleteAsync();
            }
            else
                await CreateMessage(this, $"Nie ma takiej postaci.");

        }

        [Command(">Profile")]
        public async Task Profile() => await ViewProfile(Context.User.Id, Context);
        [Command(">Profile")]
        public async Task Profile(IUser user) => await ViewProfile(user.Id, Context);
        [Command(">Profile")]
        public async Task Profile(ulong u) => await ViewProfile(u, Context);

        public async Task ViewProfile(ulong u, SocketCommandContext context)
        {
            try
            {
                Console.WriteLine("Coś jebło");
                var (get, ch) = CharacterSerializer.ReadBoard(u.ToString());
                if (get)
                {
                    var user = Program.instance.client.GetUser(u);

                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithAuthor(user == null ? context.User : user)
                         .WithColor(ch.color == null ? Color.Blue : GetColor(ch.color))
                         .WithTitle($"{ch.name} - {ch.power}");
                    
                    if(ch.age != null)
                         embed.AddField("Wiek", ch.age, false);

                    if (ch.desc != null)
                    {
                        CharacterSerializer.VerifyValue(ch.desc, out string v);
                        embed.WithDescription(v);
                    }

                    if (ch.url != null)
                        embed.WithImageUrl(ch.url);

                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                    await CreateMessage(context, "Nie znaleziono postaci.");
            }
            catch(Exception E) { Console.WriteLine(E); }
        }
    }
}