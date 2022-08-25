using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Rayiah.Handlers;
using System;
using System.Threading.Tasks;
using Rayiah.Managers;
using Rayiah.Objects.Abstracts;

namespace Rayiah
{
    internal class RayiahCore : RayiahObject
    {
        static RayiahCore instance;

        public override string Name => GetType().Name;

        public DiscordSocketClient client { get; private set; }
        public CommandService service { get; private set; }
        public IServiceProvider provider { get; private set; }

        public bool ready = false;
        public bool updateData = false;
        public string botName = "Rayiah";

        DiscordSocketConfig socketConfig;
        CommandServiceConfig commandConfig;

        public RayiahCore(DiscordSocketConfig socketConfig, CommandServiceConfig commandConfig)
        {
            instance = this;
            this.socketConfig = socketConfig;
            this.commandConfig = commandConfig;
        }

        public static RayiahCore GetInstance() => instance;

        /// <summary>
        /// Core Rayiah. Funkcja, która działa wiecznie.
        /// </summary>
        /// <returns></returns>
        public async Task StartBot()
        {
            try
            {
                using (client = new DiscordSocketClient(socketConfig))
                {
                    if (!System.IO.File.Exists(Paths.RES + "ID.txt")) throw new Exception("File doesn't exist: " + Paths.RES + "ID.txt");
                    var text = System.IO.File.ReadAllText("./Resources/ID.txt");
                    if (string.IsNullOrWhiteSpace(text)) throw new Exception("ID file is empty: " + Paths.RES + "ID.txt");

                    Printl("Update global components? (T/*)");
                    updateData = Console.ReadKey().Key == ConsoleKey.T;
                    Console.Clear();

                    // Login
                    await client.LoginAsync(TokenType.Bot, text).ContinueWith((result) => {
                        if (result.Exception != null)
                            Console.WriteLine(result);
                    });

                    await client.StartAsync().ContinueWith((result) => {
                        if (result.Exception != null)
                            Console.WriteLine(result);
                    });

                    // Handlers / Dependency Injection
                    service = new CommandService(commandConfig);
                    provider = BuildServiceProvider();

                    client.Ready += InitializeProviderServices;

                    // Late init
                    Help.CreateInstance();
                    Container.CreateInstance().InitializeModules();
                    Authorization.CreateInstance();

                    // Waiting for components initialization
                    while (!ready) { await Task.Delay(25); }

                    Status.SetDefaultStatus();

                    Printl(m: $"Loaded {Authorization.Instance.AuthorizedUsersCount} users with permissions", origin: Container.Instance.Name);

                    PrintC("[--------------]", ConsoleColor.DarkCyan);
                    PrintC($" {botName} activated.", ConsoleColor.Cyan);
                    PrintC("[--------------]", ConsoleColor.DarkCyan);

                    await Task.Delay(-1);
                }
            }
            catch (Exception E) { Console.Write(E.Message); }
        }

        /// <summary>
        /// Handlers and components Initialization.
        /// </summary>
        async Task InitializeProviderServices()
        {
            if (ready) return;
            if (updateData)
            {
                Printl("Clearing global and guild applications...");
                var nullVal = new ApplicationCommandProperties[] { };
                await client.BulkOverwriteGlobalApplicationCommandsAsync(nullVal);
                foreach(var g in client.Guilds)
                    await g.BulkOverwriteApplicationCommandAsync(nullVal);
                Printl("Clear completed.");
            }

            await provider.GetService<CommandHandler>().InitializeAsync();
            await provider.GetService<UserCommandHandler>().InitializeAsync(updateData);
            await provider.GetService<SlashCommandHandler>().InitializeAsync(updateData);
            await provider.GetService<MessageCommandHandler>().InitializeAsync(updateData);
            await provider.GetService<UsersHandler>().InitializeAsync();
            ready = true;
        }

        /// <summary>
        /// Build collection of handlers and components (Dependency Injection).
        /// </summary>
        /// <returns></returns>
        IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(client)
            .AddSingleton(service)
            .AddSingleton<MessageCommandHandler>()
            .AddSingleton<SlashCommandHandler>()
            .AddSingleton<UserCommandHandler>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<DynamicMessagesHandler>()
            .AddSingleton<UsersHandler>()
            .BuildServiceProvider();
    }
}
