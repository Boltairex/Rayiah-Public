using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rayiah.Managers;
using Rayiah.Objects.Abstracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rayiah.Handlers
{
    class UserCommandHandler : HandlerBase
    {
        private readonly DiscordSocketClient client;
        private readonly IServiceProvider services;

        public UserCommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            this.client = client;
            this.services = services;
        }


        public override async Task InitializeAsync() {
            await InitializeAsync(false);
        }

        public async Task InitializeAsync(bool withOverwrite)
        {
            client.UserCommandExecuted += HandleUserCommandAsync;

            if (!withOverwrite) return;

            UserCommandBuilder builder1 = new UserCommandBuilder()
                .WithName("addAuthority")
                .WithDMPermission(false);

            UserCommandBuilder builder2 = new UserCommandBuilder()
                .WithName("removeAuthority")
                .WithDMPermission(false);

            List<UserCommandProperties> commands = new List<UserCommandProperties>();

            commands.Add(builder1.Build());
            commands.Add(builder2.Build());

            client.CreateGlobalApplicationCommandAsync(commands[0]);
            client.CreateGlobalApplicationCommandAsync(commands[1]);
        }

        public async Task HandleUserCommandAsync(SocketUserCommand socket)
        {
            if (socket.User is null)
            {
                await socket.RespondAsync("Unhandled exception.");
                return;
            }

            if(socket.CommandName == "addAuthority")
            {
                Console.WriteLine(socket.User.Id+"  "+socket.Data.Member.Id);
                var result = Authorization.Instance.AddAuthorizedUser(socket.User.Id, socket.Data.Member.Id);

                switch (result)
                {
                    case Authorization.EndResult.Success:
                        await socket.RespondAsync("Użytkownik dodany.", ephemeral: true);
                        break;

                    case Authorization.EndResult.NoActionNeeded:
                        await socket.RespondAsync("Użytkownik posiada już permisje.", ephemeral: true);
                        break;

                    case Authorization.EndResult.CallerNotAuthorized:
                        await socket.RespondAsync("Nie masz permisji.", ephemeral: true);
                        break;
                }
            }
            else if(socket.CommandName == "removeAuthority")
            {
                var result = Authorization.Instance.RemoveAuthorizedUser(socket.User.Id, socket.Data.Member.Id);

                switch (result)
                {
                    case Authorization.EndResult.Success:
                        await socket.RespondAsync("Użytkownik usunięty.", ephemeral: true);
                        break;

                    case Authorization.EndResult.NoActionNeeded:
                        await socket.RespondAsync("Użytkownik nie posiada żadnych permisji.", ephemeral: true);
                        break;

                    case Authorization.EndResult.CallerNotAuthorized:
                        await socket.RespondAsync("Nie masz permisji.", ephemeral: true);
                        break;
                }
            }
        }

        public override void Dispose() {
            return; 
        }
    }
}
