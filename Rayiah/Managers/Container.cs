using Discord;
using Discord.Commands;
using Discord.Rest;
using Rayiah.Modules.Commands.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Rayiah.Tools;
using Rayiah.Objects.Abstracts;

namespace Rayiah.Managers
{
    // Do naprawy
/*    public class User
    {
        public ulong ID;
        public RestUserMessage Mess;
        public UserSession Session;
        public Task TimerObject;

        public User() { TimerObject = Timer(); TimerObject.Start(); }

        async Task Timer()
        {
            await Task.Delay(30000);
            Container.Instance.Users.Remove(this);
            await Mess.DeleteAsync();
            TimerObject.Dispose();
        }
    }

    public class UserSession
    {
        List<object> UsedObjects = new List<object>();
    }*/

    public class Container : ManagerBase
    {
        public static Container Instance { get; private set; } = new Container();

        #region Colors - embedColors
        public static EmbedBuilder embedColors { get; } = new EmbedBuilder()
          .WithTitle("Kolory")
          .WithColor(Color.Orange)
          .WithDescription("Do kolorów można się odwoływać na dwa sposoby. Po nazwie, i po ID.")
          .AddField("red", "0", true)
          .AddField("dark red", "1", true)
          .AddField("blue", "2", true)
          .AddField("dark blue", "3", true)
          .AddField("green", "4", true)
          .AddField("dark green", "5", true)
          .AddField("orange", "6", true)
          .AddField("dark orange", "7", true)
          .AddField("light orange", "8", true)
          .AddField("dark grey", "9", true)
          .AddField("darker grey", "10", true)
          .AddField("light grey", "11", true)
          .AddField("lighter grey", "12", true)
          .AddField("gold", "13", true)
          .AddField("purple", "14", true)
          .AddField("dark purple", "15", true)
          .AddField("magenta", "16", true)
          .AddField("dark magenta", "17", true)
          .AddField("teal", "18", true)
          .AddField("dark teal", "19", true)
          .AddField("!!!", "Inne => Teal", true);
        #endregion

        public static Random RandomSystem { get; } = new Random();

        public static ulong BotID { get; private set; } = 698537465367232604;
        public static ulong PNGStorageID { get; private set; } = 724259179157651467;

        public static string[] BannedCharacters = new string[8] { ",", ".", "/", @"\", "-", "_", "\"", "'" };
        public static char[] AllChars = new char[52] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'r', 's', 't', 'u', 'q', 'w', 'v', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'Q', 'W', 'V', 'X', 'Y', 'Z' };

        Container()
        {
            //Users = new List<User>();
        }

        public static Container CreateInstance() => Instance;

        public static string GetBannedCharacters()
        {
            string s = "\"";
            foreach (string str in BannedCharacters)
                s += $" {str}";
            return s;
        }

        public static bool BannedCharsFilter(string s)
        {
            if (s.Contains('"')) return true;
            foreach (string banned in BannedCharacters)
                if (s.Contains(banned))
                    return true;
            return false;
        }

        public IUser GetUser(ulong userID, IGuild guild)
        {
            return guild.GetUserAsync(userID).GetAwaiter().GetResult();
        }

        public IUser GetUser(ulong userID, ulong guildID)
        {
            return RayiahCore.GetInstance().client.GetGuild(guildID).GetUser(userID);
        }

        public IUser GetUser(ulong userID)
        {
            return RayiahCore.GetInstance().client.GetUser(userID);
        }

        public Container InitializeModules()
        {
            Paths.CreateInstance();
            Help.CreateInstance();

            //Common
            Directory.CreateDirectory(Paths.RES);
            Directory.CreateDirectory(Paths.TEXPath);
            Directory.CreateDirectory(Paths.TEMPPath);

            //Rayiah Galaxy
            /*
                Directory.CreateDirectory(GALAXIESPath);
                Directory.CreateDirectory(GALPaths.RESPath);
            */

            //CardsAgainstHumanity
            /*
                Directory.CreateDirectory(DECPath);
            */

            //AeriaStories
            /*   
                Console.WriteLine(AeriaStories.Init.SafetyCheck());
                Console.WriteLine(AeriaStories.Init.Paths.Resources()); // Inicjalizacja z Callbackiem
            */

            //Impostors
            if (!File.Exists("./Impostors.json")) File.Create("./Impostors.json");
            var (ck, i) = Impostors.Load();
            
            if (ck) {
                WritingTools.UsersToRoll = i.ply;
                WritingTools.Selected = i.impostors;
            }

            return this;
        }

        public static System.Drawing.Color GetRandomColor()
        {
            return System.Drawing.Color.FromArgb(
                red: RandomSystem.Next(1, 255),
                green: RandomSystem.Next(1, 255),
                blue: RandomSystem.Next(1, 255));
        }

        public bool CheckUserPermission(SocketCommandContext Context, GuildPermission Permission) => Context.Guild.CurrentUser.GuildPermissions.Has(Permission);

        public static IRole[] GetUserRoles(IGuildUser user)
        {
            List<IRole> roles = new List<IRole>();
            foreach(ulong u in user.RoleIds)
                roles.Add(user.Guild.GetRole(u));

            return roles.ToArray();
        }
    }
}