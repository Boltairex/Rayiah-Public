using Discord.Commands;
using Discord.WebSocket;
using Rayiah.Managers;
using Rayiah.Objects.Abstracts;
using Rayiah.Objects.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Rayiah.Handlers
{
    /// <summary>
    /// CommandHandler odpowiada za rozpoznawanie komend na chatcie, oraz reagowania na nie.
    /// </summary>
    public class CommandHandler : HandlerBase
    {
        //[Save]
        public Dictionary<ulong, char[]> prefixes = new Dictionary<ulong, char[]>();

        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;

        int arg = 0;

        /// <summary>
        /// Bazowy konstruktor zasilany przez DI.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="commands"></param>
        /// <param name="client"></param>
        public CommandHandler(IServiceProvider services, CommandService commands, DiscordSocketClient client)
        {
            this.commands = commands;
            this.services = services;
            this.client = client;
        }

        /// <summary>
        /// Późna inicjalizacja handlera.
        /// </summary>
        /// <returns></returns>
        public override async Task InitializeAsync()
        {
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            //Console.WriteLine(outp);
            client.MessageReceived += HandleCommandAsync;
        }

        /// <summary>
        /// Funkcja do sprawdzania prefixów gildyjnych.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CheckGuildPrefix(ref SocketCommandContext context)
        {
            if (!prefixes.ContainsKey(context.Guild.Id)) return false;
            foreach (char c in prefixes[context.Guild.Id])
                if (context.Message.HasCharPrefix(c, ref arg))
                    return true;
            return false;
        }

        /// <summary>
        /// Gdy bot otrzyma wiadomość, że ktoś wysłał wiadomość.
        /// </summary>
        /// <param name="socketMessage"></param>
        /// <returns></returns>
        public async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage message) || message.Source != Discord.MessageSource.User) return;

            var context = new SocketCommandContext(client, message);

            if (message.HasCharPrefix(':', ref arg) || CheckGuildPrefix(ref context))
            {
                IResult result;
                var searchResults = commands.Search(context, arg);
                var match = await commands.ValidateAndGetBestMatch(searchResults, context, services, MultiMatchHandling.Best);

                if (match is SearchResult resultCommand)
                {
                    var resultCommandEnum = resultCommand.Commands.GetEnumerator();
                    if (resultCommandEnum.MoveNext())
                    {
                        if (CheckCommandAuthority(resultCommandEnum.Current.Command) && !Authorization.Instance.IsUserAuthorized(context.User))
                        {
                            await context.Channel.SendMessageAsync("Brak uprawnień wewnętrznych.");
                            return;
                        }
                    }
                }
                else if (match is MatchResult resultCommands)
                {
                    if (resultCommands.Match.HasValue && resultCommands.IsSuccess && resultCommands.Match.Value is CommandMatch commandMatch)
                    {
                        if (CheckCommandAuthority(commandMatch.Command) && !Authorization.Instance.IsUserAuthorized(context.User))
                        {
                            await context.Channel.SendMessageAsync("Brak uprawnień wewnętrznych.");
                            return;
                        }
                    }
                }

                result = await commands.ExecuteAsync(context, arg, services);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await context.Channel.SendMessageAsync($"Wystąpił błąd w strukturze skryptu. Szczegóły: {result.ErrorReason}");
            }
        }

        bool CheckCommandAuthority(CommandInfo info)
        {
            var en = info.Attributes.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current is InfoAttribute attr) 
                    if (attr != null && attr.Protected)
                        return true;
            }
            return false;
        }

        public override void Dispose()
        {
            client.MessageReceived -= HandleCommandAsync;
        }
    }
}
