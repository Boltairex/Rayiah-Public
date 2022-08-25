using Discord.Commands;
using Rayiah.AeriaStories;
using Rayiah.Management;
using Rayiah.AeriaStories.Objects;
using static Rayiah.Tools.RayiahUtilites;
using System.Threading.Tasks;
using Rayiah.Tools;
using Rayiah.Modules.AeriaCommands;
using System;
using Discord;
using Discord.Rest;
using Rayiah;

namespace Rayiah.Modules.AeriaCommands
{
    public class Play : ModuleBase<SocketCommandContext>
    {
        [Command("Play")]
        public async Task OpenPanel()
        {
            if (!CharacterSerializer.CheckExistance(Context.User.Id.ToString()))
            {
                await CreateMessage(this, "Nie masz stworzonej postaci.", 3000);
            }

            //var (_, instance) = CharacterSerializer.ReadBoard(Context.User.Id.ToString());
            //GamePanel Panel = new GamePanel(instance);

            RestUserMessage x = (await Context.Channel.SendMessageAsync("this"));


            await x.AddReactionsAsync(new Emoji[2] { EmotesBank.Happy, EmotesBank.Sad });

            Console.WriteLine(x.Content);

            //Storage.Panels.Add(Panel);
        }
    }
}
