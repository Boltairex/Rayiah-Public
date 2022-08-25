using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Rayiah.Tools
{
    [Obsolete]
    class Timer
    {
        [Obsolete]
        public async Task Wait(int f)
        {
            await Task.Delay(f);
        }
    }

    static class Utilites
    {
        public static bool CompareEmbeds(Embed emb1, Embed emb2) {
            if (emb1.Color != emb2.Color) return false;
            if (emb1.Description != emb2.Description) return false;
            if (emb1.Length != emb2.Length) return false;
            return true;
        }

        /// <summary>
        /// Change JSON file into object (Deserialize). Path must include '.json' extension!
        /// <para>
        /// Exceptions:
        /// <see cref="FormatException"/>, 
        /// <see cref="FileNotFoundException"/>
        /// </para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        public static T LoadJSON<T>(string path, bool handleException = true)
        {
            string str = "";
            if (handleException) {
                try {
                    if (!path.EndsWith(".json")) throw new FormatException("Wrong extension.");
                    if (!File.Exists(path)) throw new FileNotFoundException("No file found.");
                    using (var stream = new StreamReader(path))
                        str = stream.ReadToEnd();
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
            else {
                if (!path.EndsWith(".json")) throw new FormatException("Wrong extension.");
                if (!File.Exists(path)) throw new FileNotFoundException("No file found.");
                using (var stream = new StreamReader(path))
                    str = stream.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<T>(str);
        }

        /// <summary>
        /// Change object into text (Serialize). Path must include '.json' extension!
        /// <para>
        /// Exceptions:
        /// <see cref="FormatException"/>, 
        /// <see cref="ArgumentNullException"/>
        /// </para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        public static void SaveJSON(object obj, string path)
        {
            if (!path.EndsWith(".json")) throw new FormatException("Wrong extension.");
            if (obj is null) throw new ArgumentNullException("Object is null.");
            using (var stream = new StreamWriter(path))
            {
                stream.Write(JsonConvert.SerializeObject(obj));
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];
            int ptr;
            while ((ptr = src.Read(bytes, 0, bytes.Length)) != 0)
                dest.Write(bytes, 0, ptr);
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes)) {
                using (var mso = new MemoryStream()) {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
                        CopyTo(gs, mso);
                    }
                    return Encoding.UTF8.GetString(mso.ToArray());
                }
            }
        }

        public static Color GetColor(string name)
        {
            name = name.ToLower();
            return name switch
            {
                "red" => Color.Red,
                "dark red" => Color.DarkRed,
                "blue" => Color.Blue,
                "dark blue" => Color.DarkBlue,
                "green" => Color.Green,
                "dark green" => Color.DarkGreen,
                "orange" => Color.Orange,
                "dark orange" => Color.DarkOrange,
                "light orange" => Color.LightOrange,
                "dark grey" => Color.DarkGrey,
                "darker grey" => Color.DarkerGrey,
                "light grey" => Color.LightGrey,
                "lighter grey" => Color.LighterGrey,
                "gold" => Color.Gold,
                "purple" => Color.Purple,
                "dark purple" => Color.DarkPurple,
                "magenta" => Color.Magenta,
                "dark magenta" => Color.DarkMagenta,
                "teal" => Color.Teal,
                "dark teal" => Color.DarkTeal,
                _ => Color.Default,
            };
        }

        public static Color GetColor(int i)
        {
            return i switch
            {
                0 => Color.Red,
                1 => Color.DarkRed,
                2 => Color.Blue,
                3 => Color.DarkBlue,
                4 => Color.Green,
                5 => Color.DarkGreen,
                6 => Color.Orange,
                7 => Color.DarkOrange,
                8 => Color.LightOrange,
                9 => Color.DarkGrey,
                10 => Color.DarkerGrey,
                11 => Color.LightGrey,
                12 => Color.LighterGrey,
                13 => Color.Gold,
                14 => Color.Purple,
                15 => Color.DarkPurple,
                16 => Color.Magenta,
                17 => Color.DarkMagenta,
                18 => Color.Teal,
                19 => Color.DarkTeal,
                _ => Color.Default,
            };
        }

        [Obsolete]
        public static string[] CutEveryWord(string toCut, string word, bool removeWhitespace = true)
        {
            try
            {
                List<string> Array = new List<string>();
                int pos = 0;
                int lastCut = 0;
                while (true)
                {
                    bool CutThere = true;
                    for (int wordPos = 0; wordPos < word.Length; wordPos++) {
                        if (toCut[pos + wordPos] != word[wordPos])
                        {
                            CutThere = false;
                            break;
                        }
                    }

                    if (CutThere)
                    {
                        try
                        {
                            Array.Add(toCut[lastCut..pos]);
                            lastCut = pos + 1;
                        }
                        catch (Exception E) { Console.WriteLine(E); }
                    }
                    pos++;
                    if (pos >= toCut.Length)
                        break;
                }

                if (removeWhitespace)
                {
                    for (int x = 0; x < Array.Count; x++) {
                        bool reset = false;
                        int curPos = 0;
                        while (true)
                        {
                            if (curPos == Array[x].Length)
                                break;

                            char toCheck = Array[x][curPos];
                            if (char.IsWhiteSpace(toCheck) || toCheck == '\n') {
                                Array[x] = Array[x].Remove(curPos);
                                reset = true;
                            }
                            else
                                break;

                            if (reset)
                                curPos = 0;
                            else
                                curPos++;
                        }
                    }
                }

                return Array.ToArray();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Save date format for Linux and Windows
        /// </summary>
        public static string GetCurrentDate { get => $"{DateTime.Now.ToString().Replace(':', '-').Replace('/', '-')}"; }

        public static double CalculateSimilarity(this string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        /// <summary>
        /// Returns the number of steps required to transform the source string
        /// into the target string.
        /// </summary>
        static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            if (sourceWordCount == 0) return targetWordCount;
            if (targetWordCount == 0) return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }
            return distance[sourceWordCount, targetWordCount];
        }
    }
}