using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Rayiah.Handlers;
using Rayiah.Tests;
using System;
using System.Collections.Generic;

namespace Rayiah
{
    internal class Program
    {
        static bool debugMode = false;
        static RayiahCore core;

        static void Main()
        {
            if (debugMode)
            {
                new TestMain();
                return;
            }

            DiscordSocketConfig socketConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.Guilds | GatewayIntents.AllUnprivileged,
                AlwaysDownloadUsers = true
            };

            CommandServiceConfig commandConfig = new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false
            };

            try
            {
                core = new RayiahCore(socketConfig, commandConfig);
                core.StartBot().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}