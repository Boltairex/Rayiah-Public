using Discord;
using Discord.WebSocket;
using Rayiah.Objects.Abstracts;
using System.Threading.Tasks;

namespace Rayiah.Modules.UserCommands
{
    public class GetMessageIDCommand : MessageCommandBase
    {
        public override async Task ExecuteAsync(SocketMessageCommand socket, SocketUser user)
        {
            await socket.RespondAsync(text: socket.Data.Message.Id.ToString(), ephemeral: true);
        }

        protected override MessageCommandBuilder GetCommand()
        {
            MessageCommandBuilder command = new MessageCommandBuilder()
                .WithName("getID")
                .WithDMPermission(false);

            return command;
        }
    }
}
