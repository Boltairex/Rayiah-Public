using Discord;
using Newtonsoft.Json;
using Rayiah.Modules.Commands.Writing;
using Rayiah.Objects.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Rayiah.Managers
{
    internal class Impostors : ManagerBase
    {
        internal static Impostors Instance { get; private set; } = new Impostors();

        public ITextChannel Channel;
        public IMessage LastMessage;

        Impostors()
        {
            Channel = null;
            LastMessage = null;
        }

        public async Task<bool> WriteOnChannel(string s)
        {
            if (Channel != null)
                return Channel.SendMessageAsync(s).GetAwaiter().GetResult() != null;
            return false;
        }

        public async void RevertMessage()
        {
            if (LastMessage != null)
            {
                try
                {
                    await LastMessage.DeleteAsync();
                } catch { }
                LastMessage = null;
            }
        }

        public static (bool, ImpostorInfo) Load()
        {
            try
            {
                return (true, JsonConvert.DeserializeObject<ImpostorInfo>(new StreamReader("./Impostors.json").ReadToEnd()));
            }
            catch { return (false, new ImpostorInfo()); }
        }

        public static void Save(ImpostorInfo info)
        {
            using (StreamWriter Writer = new StreamWriter("./Impostors.json"))
            {
                Writer.Write(JsonConvert.SerializeObject(info));
            }
        }

        public bool VerifyImpostor(ulong u)
        {
            return WritingTools.Selected.Contains(u);
        }

        public List<ulong> RollImpostors()
        {
            int c = new Random().Next(1, 2);
            if (c >= WritingTools.UsersToRoll.Count) return WritingTools.UsersToRoll;

            List<ulong> impostors = new List<ulong>();
            for (int x = 0; x < c; x++)
            {
                ulong u = 0;
                do
                {
                    int i = new Random().Next(0, WritingTools.UsersToRoll.Count);
                    u = WritingTools.UsersToRoll[i];
                    Console.WriteLine(i + ": " + u);
                }
                while (impostors.Contains(u) || u == 0);
                if (impostors == null)
                    impostors = new List<ulong>();
                impostors.Add(u);
            }

            return impostors; // Kinda sus.
        }

        public IUser[] GetUsers(List<ulong> us, IGuild guild)
        {
            List<IUser> users = new List<IUser>();
            foreach (ulong u in us)
            {
                if (u == 0) continue;
                users.Add(Container.Instance.GetUser(u, guild));
            }
            return users.ToArray();
        }
    }

    public struct ImpostorInfo
    {
        public List<ulong> ply;
        public List<ulong> impostors;
    }
}