using Discord.WebSocket;
using Rayiah.Managers;
using Rayiah.Objects.Abstracts;
using Rayiah.Objects.Structs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rayiah.Handlers
{
    class UsersHandler : HandlerBase
    {
        private readonly DiscordSocketClient client;
        private readonly IServiceProvider services;

        public UsersHandler(DiscordSocketClient client, IServiceProvider services) {
            this.client = client;
            this.services = services;

            client.UserJoined += UserJoined;
            client.GuildMemberUpdated += GuildMemberUpdated;
        }

        public override Task InitializeAsync()
        {
            return base.InitializeAsync();
        }

        public async Task GatherUsersData(SocketGuild guild)
        {
            List<UserRolesData> newData = new List<UserRolesData>();

            foreach (SocketGuildUser user in guild.Users)
            {
                newData.Add(new UserRolesData(user.Id, user.Roles.GetEnumerator()));
            }
            RolesBackup.CreateInstance().SaveData(newData, guild.Id, true);
        }

        async Task GuildMemberUpdated(Discord.Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser arg2)
        {
            RolesBackup.CreateInstance().UpdateUser(arg2.Guild.Id, arg2.Id, arg2.Roles);
        }

        async Task UserJoined(SocketGuildUser arg)
        {
            if (RolesBackup.CreateInstance().ContainsUser(arg.Guild.Id, arg.Id))
            {
                ulong[] roles = RolesBackup.CreateInstance().GetUserPermisions(arg.Guild.Id, arg.Id);
                if (roles.Length != 0)
                {
                    await arg.AddRolesAsync(roles, new Discord.RequestOptions() { RetryMode = Discord.RetryMode.RetryRatelimit });
                }
            }
        }

        public override void Dispose()
        {
            client.UserJoined -= UserJoined;
            client.GuildMemberUpdated -= GuildMemberUpdated;
        }
    }
}
