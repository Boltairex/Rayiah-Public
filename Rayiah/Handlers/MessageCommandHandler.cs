using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rayiah.Managers;
using Rayiah.Objects.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rayiah.Handlers
{
    public class MessageCommandHandler : HandlerBase
    {
        Dictionary<string, MessageCommandBase> registry = new Dictionary<string, MessageCommandBase>();

        private readonly DiscordSocketClient client;
        private readonly IServiceProvider services;

        public MessageCommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            this.client = client;
            this.services = services;
        }

        public override async Task InitializeAsync()
        {
            await InitializeAsync(false);
        }

        public async Task InitializeAsync(bool withOverwrite, bool withOverwriteGuilds = true)
        {
            withOverwriteGuilds = withOverwrite;
            var globalCommandsToLoad = new List<MessageCommandProperties>();
            var guildCommandsToLoad = new Dictionary<ulong, List<MessageCommandProperties>>();

            Printl("Inicjalizowane komend" + (withOverwrite ? " z rejestracją." : "."));
            Type t = typeof(MessageCommandBase);
            foreach (Type type in Assembly.GetAssembly(t).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(t)))
            {
                var message = (MessageCommandBase)Activator.CreateInstance(type);
                message.Initialize();
                registry.Add(message.Command.Name.Value.ToLower(), message);

                if (withOverwrite && message.GuildID.Length == 0)
                {
                    globalCommandsToLoad.Add(message.Command);
                }
                else if (withOverwriteGuilds)
                {
                    foreach (ulong u in message.GuildID)
                    {
                        if (guildCommandsToLoad.ContainsKey(u))
                            guildCommandsToLoad[u].Add(message.Command);
                        else
                            guildCommandsToLoad.Add(u, new List<MessageCommandProperties>() { message.Command });
                    }
                }
            }

            if (globalCommandsToLoad.Count > 0)
            {
                Printl("Aktualizacja " + globalCommandsToLoad.Count + " globalnych.");
                foreach (MessageCommandProperties p in globalCommandsToLoad)
                {
                    client.CreateGlobalApplicationCommandAsync(p).ContinueWith((socket) =>
                    {
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
                    foreach (KeyValuePair<ulong, List<MessageCommandProperties>> kv in guildCommandsToLoad)
                    {
                        var guild = client.GetGuild(kv.Key);
                        if (guild == null)
                            continue;
                        foreach (MessageCommandProperties p in kv.Value)
                        {
                            guild.CreateApplicationCommandAsync(p);
                        }
                    }
                }
            }
            client.MessageCommandExecuted += HandleMessageCommandAsync;
        }

        public async Task HandleMessageCommandAsync(SocketMessageCommand socket)
        {
            SocketUser user = socket.User;

            Console.WriteLine(socket.CommandName.ToLower());
            if (!registry.ContainsKey(socket.CommandName.ToLower()))
            {
                await socket.GetOriginalResponseAsync().Result.DeleteAsync();
                return;
            }

            MessageCommandBase commandBase = registry[socket.CommandName.ToLower()];//.ExecuteAsync(socket, user);

            if (commandBase.RequireAuthorization)
                if(!Authorization.Instance.IsUserAuthorized(socket.User))
                {
                    await socket.RespondAsync(text: "Nie masz permisji.", ephemeral: true);
                    return;
                }

            await commandBase.ExecuteAsync(socket, user);
        }

        public override void Dispose()
        {
            client.MessageCommandExecuted -= HandleMessageCommandAsync;
            return;
        }
    }
}