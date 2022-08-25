using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rayiah.Objects.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rayiah.Handlers
{
    /// <summary>
    /// SlashCommandHandler odpowiada za komendy z autouzupełnieniem (pisanych po /). Ładuje ręcznie każdą klasę, która zawera <see cref="SlashCommandBase"/> i przygotowuje do działania.
    /// </summary>
    public class SlashCommandHandler : HandlerBase
    {
        private readonly DiscordSocketClient client;
        private readonly IServiceProvider services;

        Dictionary<string, SlashCommandBase> registry = new Dictionary<string, SlashCommandBase>();

        /// <summary>
        /// Inicjalizacja przez DI.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="commands"></param>
        /// <param name="services"></param>
        public SlashCommandHandler(DiscordSocketClient client, IServiceProvider services) {
            this.client = client;
            this.services = services;
        }

        public override async Task InitializeAsync()
        {
            await InitializeAsync(false, true);
        }

        /// <summary>
        /// Późnia inicjalizacja, która odświeża Slash Komendy i buduje słownik z istniejących klas w assembly.
        /// </summary>
        /// <param name="withOverwrite"></param>
        /// <param name="withOverwriteGuilds"></param>
        /// <returns></returns>
        public async Task InitializeAsync(bool withOverwrite, bool withOverwriteGuilds = true) 
        {
            // Tymczasowe
            withOverwriteGuilds = withOverwrite;
            try
            {
                var globalCommandsToLoad = new List<SlashCommandProperties>();
                var guildCommandsToLoad = new Dictionary<ulong, List<SlashCommandProperties>>();

                Printl("Inicjalizowane komend" + (withOverwrite ? " z rejestracją." : "."));
                Type t = typeof(SlashCommandBase);
                foreach (Type type in Assembly.GetAssembly(t).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(t)))
                {
                    var slash = (SlashCommandBase)Activator.CreateInstance(type);
                    slash.Initialize();
                    registry.Add(slash.Command.Name.Value.ToLower(), slash);

                    if (withOverwrite && slash.GuildID.Length == 0)
                    {
                        globalCommandsToLoad.Add(slash.Command);
                    }
                    else if (withOverwriteGuilds)
                    {
                        foreach (ulong u in slash.GuildID)
                        {
                            if (guildCommandsToLoad.ContainsKey(u))
                                guildCommandsToLoad[u].Add(slash.Command);
                            else
                                guildCommandsToLoad.Add(u, new List<SlashCommandProperties>() { slash.Command });
                        }
                    }
                }

                if (globalCommandsToLoad.Count > 0)
                {
                    Printl("Aktualizacja " + globalCommandsToLoad.Count + " globalnych.");
                    foreach (SlashCommandProperties p in globalCommandsToLoad) {
                        client.CreateGlobalApplicationCommandAsync(p).ContinueWith((socket) => {
                            Printl(socket.Result.Name + " zainicjalizowany.");
                        });
                    }
                }

                if (guildCommandsToLoad.Count > 0)
                {
                    ApplicationCommandProperties[] empty = new ApplicationCommandProperties[0];
                    var en = client.Guilds.GetEnumerator();
                    while (en.MoveNext())
                        await en.Current.BulkOverwriteApplicationCommandAsync(empty);
                    if (guildCommandsToLoad.Count != 0)
                    {
                        Printl("Aktualizacja " + guildCommandsToLoad.Count + " gildyjnych.");
                        foreach (KeyValuePair<ulong, List<SlashCommandProperties>> kv in guildCommandsToLoad)
                        {
                            var guild = client.GetGuild(kv.Key);
                            if (guild == null)
                                continue;
                            foreach (SlashCommandProperties p in kv.Value) {
                                guild.CreateApplicationCommandAsync(p);
                            }
                        }
                    }
                }
            }catch(Exception e) { Console.WriteLine(e); }
            client.SlashCommandExecuted += HandleSlashCommandAsync;
        }

        /// <summary>
        /// Odbieranie rozkazów z discorda, czyli gdy ktoś użyje Slash Komend.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public async Task HandleSlashCommandAsync(SocketSlashCommand interaction)
        {
            IUser author = interaction.User;
            if (registry.ContainsKey(interaction.Data.Name.ToLower()))
                await registry[interaction.Data.Name.ToLower()].ExecuteAsync(interaction, author);
            else
                await interaction.GetOriginalResponseAsync().Result.DeleteAsync();
        }

        /// <summary>
        /// By nie pisać tego samego kodu, klasa może przekierować zapytanie do innej, by ona obsłużyła zapytanie.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="interaction"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        public async Task ReHandleTo(string str, SocketSlashCommand interaction, IUser author) 
        {
            str = str.ToLower();
            if (registry.ContainsKey(str))
                await registry[str].ExecuteAsync(interaction, author);
            else
                await interaction.GetOriginalResponseAsync().Result.DeleteAsync();
        }

        public override void Dispose() {
            registry.Clear();
        }
    }
}