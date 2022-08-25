using Newtonsoft.Json;
using Rayiah.AeriaStories.Objects;
using Rayiah.Management;
using System.Collections.Generic;
using System.IO;
using static Rayiah.Tools.RayiahUtilites;
using Discord.Commands;
using System.Threading.Tasks;

namespace Rayiah.AeriaStories
{
    public static class CharacterSerializer
    {
        public static List<ulong> ToDelete = new List<ulong>();

        public static bool VerifyValue(string value, out string outVal)
        {
            bool usable = true;
            if (string.IsNullOrEmpty(value) || value == "Disabled" || value == "None")
                usable = false;
            else if (value.Length > 1024)
                value = value.Substring(0, 1019) + "...";

            outVal = value;
            return usable;
        }

        public static bool CheckExistance(string fileID) => File.Exists($"./{Container.AERIA}Characters/{fileID}.json");

        public static string GetJSONFile(string fileID)
        {
            if (CheckExistance(fileID))
            {
                using (StreamReader Reader = new StreamReader($"./{Container.AERIA}Characters/{fileID}.json"))
                {
                    string json = Reader.ReadToEnd();
                    return json;
                }
            }
            else
                return "None";
        }

        public static bool RequestDelete(ulong u)
        {
            if (CheckExistance(u.ToString()))
            {
                File.Delete($"./{Container.AERIA}Characters/{u}.json");
                return true;
            }
            else
                return false;
        }

        public static void SaveBoard(CharacterInstance board, ulong u)
        {
            using StreamWriter newStream = new StreamWriter($"./{Container.AERIA}Characters/{u}.json");
            JsonSerializer Serializer = new JsonSerializer();
            Serializer.Serialize(newStream, board);
        }

        public static (bool, CharacterInstance) ReadBoard(string fileID) => ReadFile($"{Container.AERIA}Characters/{fileID}.json");

        static (bool, CharacterInstance) ReadFile(string fullPath)
        {
            string JsonText = "";
            using (StreamReader newStream = new StreamReader(fullPath))
            {
                JsonText = newStream.ReadToEnd();
            }
            return (true, JsonConvert.DeserializeObject<CharacterInstance>(JsonText));
        }

        public static void RequestCheck(ulong u, SocketCommandContext context) => CheckDeleteConfirm(u,context).GetAwaiter();

        public static async Task CheckDeleteConfirm(ulong u, SocketCommandContext context)
        {
            foreach (ulong c in ToDelete) {
                if (c == u) {
                    bool b = RequestDelete(u);
                    if (b) { await CreateMessage(context, "Usunięto postać.", 3000); ToDelete.Remove(u); }
                    else await CreateMessage(context, "Postać nie istnieje w rejestrze.");
                    return;
                } 
            }
        }
    }
}