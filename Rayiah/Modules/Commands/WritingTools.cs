using Discord;
using Discord.Commands;
using Rayiah.Managers;
using Rayiah.Objects.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rayiah.Modules.Commands.Writing
{
    [Help("Writing", "Narzędzia często pisarskie lub roleplay'owe.",6)]
    public class WritingTools : RayiahModule<SocketCommandContext>
    {
        Dictionary<ulong, CancellationTokenSource> tokens = new Dictionary<ulong, CancellationTokenSource>();

        public static List<ulong> UsersToRoll = new List<ulong>();
        public static List<ulong> Selected = new List<ulong>();

        [Command("Opowiedz mi historię"), Alias("OMH"), Info("Wybiera randomową historyjkę z bazy danych.")]
        public async Task TellStory()
        {
            if(!tokens.ContainsKey(Context.Channel.Id))
            {
                var tokenSource = new CancellationTokenSource();
                tokens.Add(Context.Channel.Id, tokenSource);

                await Task.Run(() => Dialog(Context.Channel.Id));
            }
        }

        async Task Dialog(ulong u)
        {
            string[] text;
            var dir = new DirectoryInfo($"{Paths.RES}Stories").EnumerateFiles().ToList();
            int i = Container.RandomSystem.Next(0, dir.Count);

            using (StreamReader Read = new StreamReader(dir[i].FullName))
            {
                List<string> strings = new List<string>();
                while (Read.EndOfStream)
                    strings.Add(await Read.ReadLineAsync());
                text = strings.ToArray();
            }

            for(int x = 0;; x++)
            {
                if (tokens[u].IsCancellationRequested) break;
                await Context.Channel.SendMessageAsync(text[x]);
                if (x < text.Length) await Task.Delay(2000);
                else break;
            }

            tokens.Remove(u);
        }

        [Command("Stop"), Info("Zatrzymuje opowiadanie historii przez bota.")]
        public async Task StopStory()
        {
            if (tokens.ContainsKey(Context.Channel.Id))
            {
                tokens[Context.Channel.Id].Cancel();
                await Context.Channel.SendMessageAsync("No już no...");
            }
            else
                await Context.Channel.SendMessageAsync("Przecież nic nie mówię... Dzban.");
        }

        [Command("Remove"), Info("Wycofanie się z losowania na Impostora.")]
        public async Task Remove()
        {
            UsersToRoll.Remove(Context.User.Id);
        }

        [Command("JoinToRoll"), Alias("JTR"), Info("Dołączenie do losowania na Impostora.")]
        public async Task AddToRoll()
        {
            if (!UsersToRoll.Contains(Context.User.Id)) UsersToRoll.Add(Context.User.Id);
            else
                await Context.Channel.SendMessageAsync("Jesteś już na liście.");
        }

        [Command("RollImpostor"), Info("Losowanie Impostora na bazie zgłoszonych osób.", true)]
        public async Task RollPerson()
        {
            try
            {
                var us = Impostors.Instance.GetUsers(Impostors.Instance.RollImpostors(), Context.Guild);
                foreach (IUser u in us)
                {
                    Selected.Add(u.Id);
                    await u.SendMessageAsync("Zostałeś wybrany do stworzenia antagonisty. Powodzenia z kryciem się, postaraj się nie zdradzić. Niepisana zasada, nie musisz się ograniczać. Kto wie, może będzie dwóch, z czego jeden jest podpuchą...?");
                }

                Impostors.Save(new ImpostorInfo() { impostors = Selected, ply = UsersToRoll });

                await Context.Channel.SendMessageAsync("Wylosowano. Sprawdźcie DM'y, oraz nie ugadujcie się między sobą czy jesteście traitorami, czy nie. To ma być tajemnica.");
            }
            catch (Exception E) { Print(E); }
        }

        [Command("Say"), Info("Umiejętność Impostora. Pozwala mówić na każdym kanale przez bota.")]
        public async Task ImpostorSay([Remainder] string text)
        {
            if (Context.Channel.Id == Impostors.Instance.Channel.Id)
            {
                await Context.Channel.SendMessageAsync("Nie można wypisywać na ten sam kanał wiadomości :P. To zabezpieczenie jest globalne, nie ważne czy jesteś impostorem, czy nie.");
                return;
            }

            if (Selected.Count == 0)
            {
                if (!Impostors.Instance.WriteOnChannel(text).Result) await Context.Channel.SendMessageAsync("Ogólnie to kanał jest nieprzypisany. Pokaż mi gdzie to napisać dzbanie.");
            }
            else
            {
                if (Impostors.Instance.VerifyImpostor(Context.User.Id))
                    if (!Impostors.Instance.WriteOnChannel(text).Result) await Context.Channel.SendMessageAsync("Ogólnie to kanał jest nieprzypisany. Pokaż mi gdzie to napisać dzbanie.");
            }
        }

        [Command("RevertMessage"), Info("Cofnięcie ostatnio napisanej komendy przez Impostora.")]
        public async Task RevertImpostorMessage()
        {
            if (Selected.Count == 0)
                Impostors.Instance.RevertMessage();
            else
                if (Impostors.Instance.VerifyImpostor(Context.User.Id)) Impostors.Instance.RevertMessage();
        }

        [Command("SetChannel"), Info("Ustawienie kanału do pisania przez Impostora.")]
        public async Task SetChannel(ulong u)
        {
            if (RayiahCore.GetInstance().client.GetChannel(u) is ITextChannel ch)
            {
                Impostors.Instance.Channel = ch;
                await Context.Channel.SendMessageAsync("Powinno działać, lepiej sobie przetestuj.");
            }
            else
                await Context.Channel.SendMessageAsync("Coś się zepsuło. Może ID jest złe?");
        }

        [Command("Roll"), Alias("R"), Info("Losuje liczbe z przedziału od 0 do podanej.")]
        public async Task RNG(int max)
        {
            await Context.Channel.SendMessageAsync($"{Container.RandomSystem.Next(0, max + 1)}");
        }

        [Command("Choose"), Alias("Ch"), Info("Pozwala na losowanie między opcjami. Wspiera formatowanie.\nFormaty: 'Tak, Nie, Może', lub 'Tak:4, Nie:2, Może", true)]
        public async Task ChooseOptions([Remainder] string options)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            var toChoose = options.Split(',');
            if (options.Contains(':'))
            {
                var toChooseList = new List<string>();
                for (int x = 0; x < toChoose.Length; x++)
                {
                    if (toChoose[x].Contains(":"))
                    {
                        string[] splitted = toChoose[x].Split(':');
                        int count = int.Parse(splitted[1]);
                        count = count > 100 ? 100 : count;
                        for (int y = 0; y < count; y++)
                            toChooseList.Add(splitted[0]);
                    }
                    else
                        toChooseList.Add(toChoose[x]);
                }
                toChoose = toChooseList.ToArray();
            }

            foreach (string s in toChoose)
            {
                if (counts.ContainsKey(s))
                    counts[s]++;
                else
                    counts.Add(s, 1);
            }

            string choosed = toChoose[Container.RandomSystem.Next(0, toChoose.Length)];

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.DarkRed)
                .WithTitle(choosed);

            float chance = counts[choosed] / (float)toChoose.Length;
            embed.WithDescription("Szanse wybranej opcji: " + (chance * 100).ToString("f2") + '%');

            await ReplyAsync("", false, embed.Build());
        }
    }
}