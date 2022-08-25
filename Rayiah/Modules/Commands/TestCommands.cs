using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rayiah.Managers;
using Rayiah.Objects.Attributes;
using Rayiah.Objects.Interfaces;
using static Rayiah.Tools.ImageTools;
using Color = Discord.Color;

namespace Rayiah.Modules.Commands
{
    public class TestCommands : RayiahModule<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync(message: "Pong! Latency: " + Context.Client.Latency);
        }

        [Command("getImg")]
        public async Task Img(string url)
        {
            var t = DownloadImage(url).Result;
            ImageConverter c = new ImageConverter();
            System.IO.File.WriteAllBytes("./tutaj.png", c.ConvertTo(t, typeof(byte[])) as byte[]);
        }

        [Command("Web")]
        public async Task GetWebsite(string Name)
        {
            var mess = await Context.Channel.SendMessageAsync("Pal wroty");

            HttpClient client = new HttpClient();
            var x = await client.GetAsync(Name);

            if (!x.IsSuccessStatusCode)
                await Context.Channel.SendMessageAsync("Dupa");
            else
            {
                var y = x.Headers.GetValues("").GetEnumerator();
                string En;
                do
                { En = y.Current; y.MoveNext(); Console.WriteLine(En); }
                while (y.Current != null);

                // Context.Channel.SendMessageAsync();
            }
        }
        /*
                [Command("EmoteID")]
                public async Task GetEmoji(Emote emote)
                {
                    await ReplyAsync(emote.Id + " : " + emote.Url + " : ");
                }*/

        [Command("Jesteś tam?")]
        public async Task Tak()
        {
            await Context.Channel.TriggerTypingAsync();
            await Task.Delay(2000);
            await Context.Channel.SendMessageAsync("Ta, jestem.");
        }

        [Command("Perm"), Info("Testuje permisje", true)]
        public async Task Perm()
        {
            await TempMessage("Masz permisje");
        }

        [Command("Parse")]
        public async Task ParseTest()
        {
            ISave.SaveClassesValues();
        }

        [Command("menu")]
        public async Task Menu()
        {
            Print("Funkcjonuje.");
            ComponentBuilder component = new ComponentBuilder();
            component.WithSelectMenu(new SelectMenuBuilder()
            {
                Options = new System.Collections.Generic.List<SelectMenuOptionBuilder>()
                {
                    new SelectMenuOptionBuilder()
                    {
                        Label = "stary",
                        Value = "pijany",
                        IsDefault = true,
                        Description="bad"
                    },
                    new SelectMenuOptionBuilder()
                    {
                        Label = "stara",
                        Value = "pijana",
                        Description="good"
                    }
                },
                Placeholder = "Bruh",
                IsDisabled = false,
            });
            Print("Wybuildował.");
            await ReplyAsync("Powinien być panel", components: component.Build());
        }

        [Command("perm"), Info("przywraca permisje", false)]
        public async Task GetPermissions()
        {
            Rayiah.Managers.Authorization.Instance.Restore(285031189956263936);
        }
    }
}
