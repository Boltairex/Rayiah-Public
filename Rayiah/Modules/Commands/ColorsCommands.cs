using Discord;
using Discord.Commands;
using Rayiah.Objects.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rayiah.Modules.Commands
{
    [Help("Colors", "Lista dostępnych kolorów w Rayiah.")]
    public class ColorsCommands : ModuleBase<SocketCommandContext> 
    {
        [Command("Colors")]
        public async Task GetColors() {
            await ReplyAsync(embed: Managers.Container.embedColors.WithAuthor(Context.User).Build());
        }
    }
}
