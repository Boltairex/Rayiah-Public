using Discord.Commands;
using System.Threading.Tasks;
using Rayiah.Ekumena;
using System.Drawing;
using System.Linq;
using System;
using static Rayiah.Tools.RayiahUtilites;
using System.Collections.Generic;
using System.IO;

namespace Rayiah.Modules
{
    public class EkumenaCommands : ModuleBase<SocketCommandContext>
    {
        [Command("?CreatePlayer")]
        public async Task CreatePlayerHelp()
        {
            await Context.Channel.SendMessageAsync("Użycie: \n" +
                "`Nazwa, Opis, Klasa, PosX:PosY, R:G:B` \n" +
                "Dwukropki oznaczają rozdzielenie stringa. \n" +
                "PosX:PosY ja podaję wam. R:G:B to kolor waszego państwa. \n");
        }

        [Command("?Classes")]
        public async Task ClassesHelp()
        {
            string str = "";
            foreach (PlayerCharacterType type in EkumenaInterpreter.Classes.Keys.ToArray())
                str += type.ToString() + " ";

            await Context.Channel.SendMessageAsync("`" + str + "`");
        }

        [Command("?Positions")]
        public async Task Positions()
        {
            string str = "";
            foreach (Point p in EkumenaLoader.SpawnPoints)
                str += p.ToString() + " ";
            Console.WriteLine(str);
        }

        public string FixString(string s)
        {
            while (s.Contains(','))
            {
                string newStr = "";
                var arr = s.Split(',');
                foreach (string str in arr)
                    newStr += str;
                s = newStr;
            }
            return s;
        }

        [Command("Generate")]
        public async Task GenerateTerrain()
        {
            try
            {
                if (File.Exists(Management.Container.TEMPPath + Context.User.Id + ".png"))
                    File.Delete(Management.Container.TEMPPath + Context.User.Id + ".png");

                var player = EkumenaStorage.GetPlayer(Context.User.Id);
                var img = EkumenaVisualizer.GenerateTerrain(player, player.Capital, new GenerateOptions(false, true, true, true), 8);

                var x = Context.User.GetOrCreateDMChannelAsync().Result;
                img.Save(Management.Container.TEMPPath + player.User.Id + ".png", System.Drawing.Imaging.ImageFormat.Png);
                await Task.Delay(100);
                await x.SendFileAsync(Management.Container.TEMPPath + player.User.Id + ".png");
                File.Delete(Management.Container.TEMPPath + Context.User.Id + ".png");
            }catch(Exception e) { Console.WriteLine(e); }
        }

        [Command("CreatePlayer")]
        public async Task CreatePlayer(string Name, string Desc, string Class, string pos, string RGB)
        {
            try
            {
                string[] XY = pos.Split(":");
                XY[1] = XY[1].Split(",")[0];
                string[] Colors = RGB.Split(":");
                Color newColor = Color.FromArgb(int.Parse(Colors[0]), int.Parse(Colors[1]), int.Parse(FixString(Colors[2])));
                Point Point = new Point(int.Parse(XY[0]), int.Parse(FixString(XY[1])));
                var cl = Enum.Parse<PlayerCharacterType>(FixString(Class));
                if (!EkumenaStorage.RegisteredPlayersSafeCheck(newColor))
                {
                    await CreateMessage(this, "Suma kolorów jest już zajęta, przesuń parę liczb i powinno być git.");
                    return;
                }

                bool goodPosition = false;
                foreach (Point p in EkumenaLoader.SpawnPoints)
                    if (p == Point)
                    {
                        goodPosition = true;
                        break;
                    }

                if (!goodPosition)
                {
                    await CreateMessage(this, "Zła pozycja startowa.");
                    return;
                }

                Dictionary<Point, IntelStruct> Intel = new Dictionary<Point, IntelStruct>();
                var Points = EkumenaLoader.CircleDetection(Point, 20);
                foreach (Point p in Points)
                    Intel.Add(p,new IntelStruct(false, true));

                Dictionary<ResourceType, int> StartResources = new Dictionary<ResourceType, int>();
                StartResources.Add(ResourceType.Food, 100);
                StartResources.Add(ResourceType.Happiness, 10);
                StartResources.Add(ResourceType.Money, 1000);
                StartResources.Add(ResourceType.Pop, 50);
                StartResources.Add(ResourceType.BuildingResource, 200);

                EkumenaPlayerStruct Player = new EkumenaPlayerStruct(
                    Context.User.Id,
                    Name,
                    Desc,
                    Intel,
                    StartResources,
                    cl,
                    Point
                   );

                if (EkumenaLoader.InitializeNewPlayer(Player, Point, newColor))
                    await CreateMessage(this, "Utworzono państwo pomyślnie.", 5000);
                else
                    await CreateMessage(this, "Wystąpił błąd.", 5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
