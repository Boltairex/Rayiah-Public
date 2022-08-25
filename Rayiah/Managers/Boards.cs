using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Rayiah.Objects.Interfaces;
using Rayiah.Objects.Structs;
using Rayiah.Tools;
using System;
using System.IO;
using System.Linq;

namespace Rayiah.Managers
{
    static class Boards
    {
        static Boards()
        {
            Paths.instance.AddTypeRecognize(typeof(CharacterBoardData), Paths.RES + "Characters");
            Paths.instance.AddTypeRecognize(typeof(UnitBoardData), Paths.RES + "Units");
        }

        public static EmbedCustomFieldData VerifyField(EmbedCustomFieldData field)
        {
            if (string.IsNullOrEmpty(field.desc))
                field.desc = "Brak";
            else if (field.desc.Length > 1024)
                field.desc = field.desc.Substring(0, 1019) + "...";

            if (string.IsNullOrEmpty(field.name))
                field.name = "Brak";
            else if (field.name.Length > 1024)
                field.name = field.name.Substring(0, 1019) + "...";

            return field;
        }

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

        public static bool CheckExistance<T>(string fileID, ulong guildID)
        {
            return File.Exists($"{Paths.instance.TypeToPath(typeof(T))}/{guildID}/{fileID}.json");
        }

        public static string GetJSONFile<T>(string fileID, ulong guildID)
        {
            if (CheckExistance<T>(fileID, guildID))
            {
                using (StreamReader Reader = new StreamReader($"{Paths.instance.TypeToPath(typeof(T))}/{guildID}/{fileID}.json"))
                {
                    string json = Reader.ReadToEnd();
                    return json;
                }
            }
            else
                return "None";
        }

        public static bool LoadJSONFile<T>(string fileID, ulong guildID, string content)
        {
            if (CheckExistance<T>(fileID, guildID))
            {
                using (StreamWriter Write = new StreamWriter($"{Paths.instance.TypeToPath(typeof(T))}/{guildID}/{fileID}.json"))
                {
                    Write.Write(content);
                    return true;
                }
            }
            else
                return false;
        }

        public static bool RequestDelete<T>(T board, ulong guildID)
        {
            if (File.Exists($"{Paths.instance.TypeToPath(typeof(T))}/{guildID}/{((IBoard)board).name}.json"))
            {
                File.Delete($"{Paths.instance.TypeToPath(typeof(T))}/{guildID}/{((IBoard)board).name}.json");
                return true;
            }
            else
                return false;
        }

        public static void SaveBoard<T>(T board, ulong guildID) where T : IBoard
        {
            using StreamWriter newStream = new StreamWriter($"{Paths.instance.TypeToPath(typeof(T))}/{guildID}/{board.name}.json");
            JsonSerializer Serializer = new JsonSerializer();
            Serializer.Serialize(newStream, board);
        }

        public static (bool, T) ReadBoard<T>(string fileID, ulong guildID, bool onlySameGuild = true) where T : IBoard
        {
            if (fileID.Contains('.'))
                fileID = fileID.Split('.')[0];

            return FindBoard<T>(fileID, guildID, onlySameGuild);
        }

        static (bool, T) FindBoard<T>(string fileID, ulong guildID, bool onlySameGuild) where T : IBoard
        {
            if (onlySameGuild)
                return ReadFile<T>($"{Paths.instance.TypeToPath(typeof(T))}/{guildID}/{fileID}.json");
            else
            {
                var guilds = new DirectoryInfo($"{Paths.instance.TypeToPath(typeof(T))}/")
                    .GetDirectories()
                    .ToArray();

                foreach (DirectoryInfo dir in guilds)
                {
                    var str = new DirectoryInfo($"{Paths.instance.TypeToPath(typeof(T))}/{dir.Name}/")
                        .GetFiles()
                        .OrderBy(T => T.Name)
                        .ToArray();

                    foreach (FileInfo info in str)
                        if (info.Name == $"{fileID}.json")
                            return ReadFile<T>(info.FullName);
                }
            }
            return (false, default(T));
        }

        /// <summary>
        /// Bez zabezpieczeń przed accessem do nieistniejacych plików.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        static (bool, T) ReadFile<T>(string fullPath)
        {
            string JsonText = "";

            using (StreamReader newStream = new StreamReader(fullPath))
                JsonText = newStream.ReadToEnd();

            return (true, JsonConvert.DeserializeObject<T>(JsonText));
        }

        public static (bool, EmbedBuilder) FormEmbedBoardBuilder(string fileID, SocketCommandContext Context)
        {
            var (get, ch) = ReadBoard<CharacterBoardData>(fileID, Context.Guild.Id, true);
            if (get)
            {
                var embed = new EmbedBuilder();

                if (VerifyValue(ch.age, out string ageVal))
                    embed.AddField("Wiek", ageVal);

                if (VerifyValue(ch.desc, out string descVal))
                    embed.AddField("Opis", descVal);

                if (VerifyValue(ch.clothes, out string clothesVal))
                    embed.AddField("Wygląd zewnętrzny", clothesVal);

                embed.WithAuthor(Context.User)
                    .WithColor(ch.color == null ? Color.LightOrange : Utilites.GetColor(ch.color))
                    .WithTitle($"{ch.name} - {ch.power}");

                if (ch.url != null)
                    embed.WithImageUrl(ch.url);

                if (ch.fields != null)
                {
                    for (int x = 0; x < ch.fields.Length; x++)
                    {
                        ch.fields[x] = VerifyField(ch.fields[x]);
                        embed.AddField(ch.fields[x].name, ch.fields[x].desc, ch.fields[x].sorted);
                    }
                }
                return (true, embed);
            }
            return (false, null);
        }

        public static (bool, Embed) FormEmbedBoard(string fileID, SocketCommandContext Context)
        {
            var (exist, eb) = FormEmbedBoardBuilder(fileID, Context);
            return (exist, exist ? eb.Build() : null);
        }
    }
}