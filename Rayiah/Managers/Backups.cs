using Discord;
using System;
using System.IO;
using System.Linq;
using Rayiah.Managers;
using Rayiah.Objects.Abstracts;

namespace Rayiah.Tools
{
    static class Backups
    {
        static Backups()
        {
            if (!Directory.Exists(Paths.BACPath))
                Directory.CreateDirectory(Paths.BACPath);
        }

        /// <summary>
        /// Zapisuje wiadomość dla konkretnej gildii. Nie dzieli na kanały.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="message"></param>
        public static void SaveBackup(IGuild guild, string message)
        {
            if (message.Length < 4) return;

            if (!Directory.Exists($"{Paths.BACPath}/{guild.Id}/"))
                Directory.CreateDirectory($"{Paths.BACPath}/{guild.Id}/");

            using (StreamWriter writer = new StreamWriter($"{Paths.BACPath}/{guild.Id}/{Rayiah.Tools.Utilites.GetCurrentDate}.txt", false, System.Text.Encoding.UTF8))
            {
                writer.Write(message);
            }
        }

        /// <summary>
        /// Zapisuje wiadomości dla konkretnej gildii. Nie dzieli na kanały.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="messages"></param>
        public static void SaveBackup(IGuild guild, string[] messages)
        {
            if ((messages.Length == 1 && messages[0].Length < 4) || messages.Length == 0) return;

            if (!Directory.Exists($"{Paths.BACPath}/{guild.Id}/"))
                Directory.CreateDirectory($"{Paths.BACPath}/{guild.Id}/");

            using (StreamWriter writer = new StreamWriter($"{Paths.BACPath}/{guild.Id}/{Rayiah.Tools.Utilites.GetCurrentDate}.txt", false, System.Text.Encoding.UTF8))
            {
                foreach (string s in messages)
                    writer.WriteLine(s);
            }
        }
        
        /// <summary>
        /// Czyszczenie folderu z backupami gildii.
        /// </summary>
        /// <param name="guild"></param>
        public static void DeleteBackupsGuild(IGuild guild)
        {
            if (Directory.Exists($"{Paths.BACPath}/{guild.Id}"))
            {
                var dir = new DirectoryInfo($"{Paths.BACPath}/{guild.Id}");
                foreach (FileInfo fi in dir.EnumerateFiles())
                    fi.Delete();
                dir.Delete();
            }
        }

        /// <summary>
        /// Czyszczenie wszystkich folderów z backupami.
        /// </summary>
        public static void DeleteBackupsGlobal()
        {
            DirectoryInfo globalDir = new DirectoryInfo($"{Paths.BACPath}");
            foreach (DirectoryInfo dir in globalDir.GetDirectories())
            {
                foreach (FileInfo fi in dir.EnumerateFiles())
                    fi.Delete();
                dir.Delete();
            }
        }

        /// <summary>
        /// Odczytywanie listy backupów gildii. Zwraca max do 30 elementów.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public static (FileInfo[], bool) GetBackupFiles(IGuild guild)
        {
            if (!Directory.Exists($"{Paths.BACPath}/{guild.Id}")) return (null, false);
            DirectoryInfo di = new DirectoryInfo($"{Paths.BACPath}/{guild.Id}");
            var files = di
                .GetFiles()
                .OrderBy(f => f.LastWriteTime)
                .Reverse()
                .ToArray();
            
            return (files[0..(files.Length > 30 ? 30 : files.Length)], true);
        }

        /// <summary>
        /// Odczytywanie backupów gildii.
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static (FileInfo, bool) GetBackupFile(IGuild guild, int index)
        {
            var (files, check) = GetBackupFiles(guild);
            if (!check) return (null, false);
            
            index = index < 0 ? 0 : (index >= files.Length ? (files.Length - 1) : index);
            Console.WriteLine(files.Length);
            return (files[index], true);
        }
    }
}