using Discord.Commands;
using Rayiah.Managers;
using System.Threading.Tasks;

namespace Rayiah.Modules.Commands
{
    public class HelpCommands : ModuleBase<SocketCommandContext>
    {
        [Command("?")]
        public async Task NewHelp([Remainder] string command = "Help")
        {
            var (x, y) = Help.Instance.GetEmbedBuilder(command.ToLower());
            if (x)
                await ReplyAsync(embed: y[0].Build());
            else
                await ReplyAsync(message: "Nie znaleziono pomocy.");
        }
    }
}