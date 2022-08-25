using Discord;
using Discord.Commands;
using Rayiah.Handlers;
using Rayiah.Managers;
using Rayiah.Objects.Attributes;
using Rayiah.Tools;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rayiah.Modules.Commands
{
    [Help("Utilities", "Komendy przydatne do zarządzania serwerem.", 4)]
    public class Utilities : RayiahModule<SocketCommandContext>
    {
        [Command("Avatar"), Info("Pobiera avatar kogoś, lub własny gdy nie wskazano użytkownika.")]
        public async Task GetAvatar(IUser User = null)
        {
            if (User is null)
                User = Context.User;

            if (User.GetAvatarUrl() != null)
                await Context.Channel.SendMessageAsync(User.GetAvatarUrl());
            else
                await Context.Channel.SendMessageAsync("Ten cieć nie ma awatara.");
        }

        [Command("Info"), Info("Wyświetla informacje o kanale. Analizuje do 5000 wiadomości.", true)]
        public async Task ChannelInfo()
        {
            await DeleteMessage();
            int messages = 0, words = 0, chars = 0;
            var tempMessages = Context.Channel.GetMessagesAsync(5000, CacheMode.AllowDownload).Flatten().ToListAsync().Result;
            int line = 1;
            int lineChars = 0;

            foreach (IMessage m in tempMessages)
            {
                if (string.IsNullOrWhiteSpace(m.Content)) continue;
                messages++;
                words++;
                int i = 0;
                do
                {
                    switch (m.Content[i])
                    {
                        case ' ':
                            words++;
                            lineChars++;
                            break;
                        case '\n':
                            line++;
                            lineChars = 0;
                            break;
                        default:
                            lineChars++;
                            chars++;
                            if (lineChars >= 60) {
                                lineChars -= 60;
                                line++;
                            }
                            break;
                    }
                    i++;
                }
                while (i < m.Content.Length);
            }

            int seconds = (int)Math.Ceiling(words / 4.58d);
            int minutes = (int)Math.Floor(seconds / 60d);
            seconds %= 60;

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor(Context.User)
                .WithColor(Color.Orange)
                .WithDescription($"- **Informacje o kanale {Context.Channel.Name}**")
                .AddField("Słowa", words, true)
                .AddField("Litery", chars, true)
                .AddField("Czas czytania", $"{minutes}m {seconds}s", false)
                .AddField("Wiadomości", messages, true)
                .AddField("Strony", ((float)line / 30).ToString("f2"), true);

            ComponentBuilder builder = new ComponentBuilder();
            ButtonBuilder bbuilder = new ButtonBuilder();
            bbuilder.WithUrl(tempMessages[tempMessages.Count - 1].GetJumpUrl())
                .WithStyle(ButtonStyle.Link)
                .WithLabel("Pierwsza wiadomość");
            builder.WithButton(bbuilder);
            await Context.Channel.SendMessageAsync("", false, embed.Build(), components: builder.Build());
        }

        [Command("Save"), Info("Zapisuje kanał do pliku tekstowego o podanej nazwie.", true)]
        public async Task SaveText([Remainder] string name = "output.txt")
        {
            await Context.Message.DeleteAsync();
            string s = "";
            var tempMessages = Context.Channel.GetMessagesAsync(5000).Flatten().Reverse();

            foreach (IMessage Message in tempMessages.ToEnumerable())
                if (Message.Content != null)
                    s += Message.Content + "\n";

            File.WriteAllText($"{Paths.TEMPPath}{name}.txt", s);
            await Context.Channel.SendFileAsync($"{Paths.TEMPPath}{name}.txt");
            File.Delete($"{Paths.TEMPPath}{name}.txt");
        }

        [Command("o")]
        public async Task Read()
        {
            using var client = new HttpClient();
            await Context.Channel.SendMessageAsync(await client.GetStringAsync(Context.Message.Attachments.First().Url));
            await Context.Message.DeleteAsync();
        }

        [Command("Channel")]
        public async Task GetChannelID([Remainder] IChannel CH)
        {
            await Context.Channel.SendMessageAsync(CH.Id.ToString());
        }

        [Command("Clear"), Info("Clearuje wiadomości bulkiem. Może usunąć do max 50 wiadomości.", true)]
        public async Task Clear(int size)
        {
            await Context.Message.DeleteAsync();

            size = Math.Clamp(size, 1, 50);
            var mess = Context.Channel.GetMessagesAsync(size, CacheMode.AllowDownload).FlattenAsync().Result;
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(mess);
            mess = mess.Reverse();

            string s = "";
            foreach (IMessage Message in mess)
            {
                if (Message.Content != null)
                    s += Message.Content + "\n";
            }

            Backups.SaveBackup(Context.Guild, s);
        }

        [Command("ClearBackups"), Alias("CB"), Info("Czyści backupy serwerowe.", true)]
        public async Task DeleteBackups()
        {
            await Context.Message.DeleteAsync();
            Backups.DeleteBackupsGuild(Context.Guild);
        }

        [Command("ClearAllBackups"), Alias("CAB"), Info("Czyści backupy globalnie.", true)]
        public async Task DeleteAllBackups()
        {
            await Context.Message.DeleteAsync();
            Backups.DeleteBackupsGlobal();
        }

        [Command("ShowBackups"), Alias("SB"), Info("Pokazuje zapisane backupy dla serwera.", true)]
        public async Task ShowBackups()
        {
            var (files, check) = Backups.GetBackupFiles(Context.Guild);
            if (!check)
                await Context.Channel.SendMessageAsync("Brak Backupów.");
            else
            {
                Console.WriteLine(files.Length);
                string s = "";
                for (int x = 0; x < files.Length; x++)
                    s += $"{x}:{files[x].Name}\n";

                await Context.Channel.SendMessageAsync(s);
            }
        }

        [Command("DownloadBackup"), Alias("DB"), Info("Pozwala na pobranie ostatniego backupu w formie pliku tekstowego.", true)]
        public async Task DownloadBackup()
        {
            var (file, check) = Backups.GetBackupFile(Context.Guild, 0);

            if (!check)
                await Context.Channel.SendMessageAsync("Brak Backupów.");
            else
                await Context.Channel.SendFileAsync(file.FullName);
        }

        [Command("DownloadBackup"), Alias("DB"), Info("Pozwala na pobranie konkretnego backupu.", true)]
        public async Task DownloadBackup(int index = -1)
        {
            var (file, check) = Backups.GetBackupFile(Context.Guild, index);

            if (!check)
                await Context.Channel.SendMessageAsync("Brak Backupów.");
            else
                await Context.Channel.SendFileAsync(file.FullName);
        }

        [Command("Backup"), Info("Przywraca ostatni backup.", true)]
        public async Task MessagesBackup()
        {
            var (file, check) = Backups.GetBackupFile(Context.Guild, 0);

            if (!check)
                await Context.Channel.SendMessageAsync("Brak Backupów.");
            else
                await Context.Channel.SendFileAsync(file.FullName);
        }

        [Command("Backup"), Info("Przywraca konkretny z listy backup.", true)]
        public async Task MessagesBackup(int index)
        {
            var (file, check) = Backups.GetBackupFile(Context.Guild, index);

            try
            {
                if (!check)
                    await Context.Channel.SendMessageAsync("Brak Backupów.");
                else
                    await Context.Channel.SendFileAsync(file.FullName);
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        [Command("CheckID")]
        public async Task CheckUserID(IUser User) => await Context.Channel.SendMessageAsync($"{User.Id}");

        [Command("AddUser"), Info("Dodaje użytkownika do zweryfikowanych.", true)]
        public async Task AddAuthorisedUser(IUser User)
        {
            var result = Authorization.CreateInstance().AddAuthorizedUser(Context.User.Id, User.Id);

            switch (result)
            {
                case Authorization.EndResult.Success:
                    await ReplyAsync("Użytkownik dodany.");
                    break;

                case Authorization.EndResult.NoActionNeeded:
                    await ReplyAsync("Użytkownik posiada już permisje.");
                    break;

                case Authorization.EndResult.CallerNotAuthorized:
                    await ReplyAsync("Nie masz permisji.");
                    break;
            }
        }

        [Command("RemoveUser"), Info("Usuwa użytkownika ze zweryfikowanych.", true)]
        public async Task RemoveAuthorisedUser(IUser User)
        {
            var result = Authorization.CreateInstance().RemoveAuthorizedUser(Context.User.Id, User.Id);

            switch (result)
            {
                case Authorization.EndResult.Success:
                    await ReplyAsync("Użytkownik usunięty.");
                    break;

                case Authorization.EndResult.NoActionNeeded:
                    await ReplyAsync("Użytkownik nie posiada żadnych permisji.");
                    break;

                case Authorization.EndResult.CallerNotAuthorized:
                    await ReplyAsync("Nie masz permisji.");
                    break;
            }
        }

        [Command("AddPrefix"), Info("Gildyjnie dodaje prefix, który rozpozna bot.\nUwaga. Nie posiada zapisywania, więc z każdym resetem bota trzeba je dodać na nowo. (To zmieni się z następnym update'm)", true)]
        public async Task AddNewPrefix(char c)
        {
            var obj = RayiahCore.GetInstance().provider.GetService(typeof(CommandHandler)) as CommandHandler;
            if (obj is null) return;
            if (obj.prefixes.ContainsKey(Context.Guild.Id))
            {
                int ptr = 0;
                char[] prefs = new char[obj.prefixes.Count + 1];
                foreach (char pref in obj.prefixes[Context.Guild.Id])
                {
                    prefs[ptr] = obj.prefixes[Context.Guild.Id][ptr++];
                    if (pref == c)
                    {
                        await TempMessage("Prefix jest już dodany.");
                        return;
                    }
                }
                prefs[prefs.Length - 1] = c;
                obj.prefixes[Context.Guild.Id] = prefs;
            }
            else
                obj.prefixes.Add(Context.Guild.Id, new char[] { c });
        }

        [Command("GatherUsersRoles"), Info("Zbiera z serwera wszystkich użytkowników i zapisuje ich role do pamięci.", true)]
        public async Task GatherUserRoles()
        {
            await (RayiahCore.GetInstance().provider.GetService(typeof(UsersHandler)) as UsersHandler).GatherUsersData(Context.Guild);
            var users = RolesBackup.instance.GetData(Context.Guild.Id);
            await ReplyAsync("Zebrano " + users.Count + " użytkowników!.");
        }

        [Command("PictureEmbed"), Alias("PEMB"), Info("Tworzy prosty embed z trzech parametrów (\"treść\", \"url obrazka\", autorzy)", true)]
        public async Task CreateEmbed(string desc, string url, [Remainder] string title = "")
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.AddField(title, desc, false)
                .WithColor((Color)Container.GetRandomColor())
                .WithImageUrl(url);
            await Context.Message.DeleteAsync();
            await ReplyAsync(embed: builder.Build());
        }

        [Command("Embed"), Alias("EMB"), Info("Tworzy prosty embed z dwóch parametrów", true)]
        public async Task CreateEmbed(string desc, [Remainder] string title = "")
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.AddField(title, desc, false)
                .WithColor((Color)Container.GetRandomColor());
            await Context.Message.DeleteAsync();
            await ReplyAsync(embed: builder.Build());
        }

        #region Outdated
        /*
        [Command("ClearTo")]
        public async Task ClearTo(string waypoint)
        {
            await ReplyAsync("Wyłączone.");
            return;
            try
            {
                await ReplyAsync("W trakcie usuwania...");
                string toFind = waypoint[0].Equals('#') ? waypoint.ToLower() : "#" + waypoint.ToLower();
                IMessage message = null;
                IMessage buffer = null;
                int c = 0;
                do
                {
                    Console.WriteLine("Okrążenie " + c);
                    int i = 0;
                    c += 50;
                    IEnumerable<IMessage> mess = null;

                    if (!(buffer is null))
                    {
                        Console.WriteLine("E");
                        mess = Context.Channel.GetMessagesAsync(buffer.Id, Direction.Before, 50, CacheMode.AllowDownload)
                            .FlattenAsync().Result;
                    }
                    else { mess = Context.Channel.GetMessagesAsync(c, CacheMode.AllowDownload).FlattenAsync().Result; }

                    if (mess is null) break;

                    foreach (IMessage m in mess)
                    {
                        if (m is null) continue;

                        if (m.Content.ToLower() == toFind)
                        {
                            message = m;
                            c -= 50 - i;
                            break;
                        }

                        i++;
                    }
                    buffer = mess.Last();
                }
                while (message is null && c < 500);
                if (c >= 500 && message is null) { await TempMessage("Nie wykryto podanej wiadomości w ostatnich 500."); return; }

                do
                {
                    Console.WriteLine("Czyszczę " + c);
                    await Clear(c);
                    c = c > 50 ? c - 50 : 0;
                }
                while (c <= 0);
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        [Command("Activity"), Info("Wskazuje aktywność osób na kanale przez ilość wysłanych wiadomości.", true)]
        public async Task GetChannelMessages()
        {
            try
            {
                var LocalMessages = Context.Channel.GetMessagesAsync(1000000).Flatten().ToEnumerable();
                var Userss = new List<string>();
                Dictionary<string, int> Users = new Dictionary<string, int>();

                foreach (IMessage M in LocalMessages)
                {
                    if (Users.ContainsKey(M.Author.Username))
                    {
                        Users[M.Author.Username] += 1;
                    }
                    else
                    {
                        Users.Add(M.Author.Username, 1);
                        Userss.Add(M.Author.Username);
                    }
                }

                var TempUserss = Userss;
                var List1 = new List<string>();
                for (int X = 0; X < TempUserss.Count; X++)
                {
                    int Highest = 0;
                    string Username = "";

                    foreach (string U in TempUserss)
                    {
                        if (Highest == 0 || Users[U] > Highest)
                        {
                            Highest = Users[U];
                            Username = U;
                        }
                    }

                    TempUserss.Remove(Username);
                    List1.Add(Username);
                }

                TempUserss = List1;

                string Output = "";
                int Index = 1;

                foreach (string U in TempUserss)
                {
                    Output += $"{Index}. {U}: {Users[U]}\n";
                    if (Output.Length > 1900)
                    {
                        await Context.Channel.SendMessageAsync(Output);
                        Output = "";
                    }
                    Index++;
                }

                await Context.Channel.SendMessageAsync(Output);
            }
            catch (Exception E) { Console.WriteLine(E); }
        }

        [Command("BanLink")]
        public async Task BanLink([Remainder] string url)
        {
            foreach (string s in AntyUzerModule.texts)
                if (s == url)
                    return;
            AntyUzerModule.texts.Add(url);
            await Context.Message.ReplyAsync("Dodano pomyślnie");
        }
        public static List<string> savedMessages = new List<string>();

        [Command("Copy")]
                public async Task CopyChannel()
                {
                    try
                    {
                        if (!Container.instance.CheckUserAuthority(Context.User))
                        {
                            await CreateMessage(this, $"Nie masz permisji.");
                            return;
                        }
                        try { await Context.Message.DeleteAsync(); } catch { }

                        savedMessages.Clear();

                        var Messages = Context.Channel.GetMessagesAsync(5000, CacheMode.AllowDownload).Flatten().ToEnumerable();
                        string Text = "";
                        foreach (IMessage Message in Messages)
                        {
                            Text += Message.Content;
                            if (Text.Length + Message.Content.Length > 2000)
                            {
                                savedMessages.Add(Text);
                                Text = "";
                            }
                        }

                        savedMessages.Add(Text);

                        Console.WriteLine(savedMessages.Count);
                        savedMessages.Reverse();
                        await Context.Channel.SendMessageAsync("Skopiowano.");
                    }
                    catch (Exception e) { Console.WriteLine(e); }
                }

                [Command("Paste")]
                public async Task PasteServer()
                {
                    if (!Container.instance.CheckUserAuthority(Context.User))
                    {
                        await CreateMessage(this, $"Nie masz permisji.");
                        return;
                    }

                    await Context.Message.DeleteAsync();
                    foreach (string Message in savedMessages)
                    {
                        await Context.Channel.SendMessageAsync(Message);
                    }

                    await Context.Channel.SendMessageAsync("Wklejono.");
                }

        [Command("ClearTo")]
        public async Task ClearTo(ulong messageId)
        {
            await Context.Channel.SendMessageAsync("W trakcie usuwania...");
            var mess = Context.Channel.GetMessagesAsync(messageId, Direction.After, 50, CacheMode.AllowDownload).FlattenAsync().Result;
            if (mess is null) { await CreateMessage(this,"Nie wykryto podanej wiadomości, albo wszystkie podane od niej wiadomości są już usunięte."); }

            Console.WriteLine(mess.Count());
            Console.WriteLine(mess.Last().Content);
            int c = mess.Count();
            do
            {
                await Clear(c);
                c = c > 50 ? c - 50 : 0;
            }
            while (c <= 0);
        }
        */
        #endregion
    }
}