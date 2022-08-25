using Discord;
using Rayiah.Objects.Abstracts;
using Rayiah.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rayiah.Managers
{
    public class Authorization : ManagerBase
    {
        internal static Authorization Instance { get; } = new Authorization();

        internal static Authorization CreateInstance() { return Instance; }

        public int AuthorizedUsersCount => authorizedUsers.Count;

        public IReadOnlyList<ulong> AuthorizedUsers => authorizedUsers;

        bool unsavedData;

        List<ulong> authorizedUsers;
        
        Authorization() {
            authorizedUsers = new List<ulong>();
            LoadAuthorizedUsers();

            AppDomain.CurrentDomain.ProcessExit += (_, __) => {
                SaveData();
            };
        }

        /// <summary>
        /// Przy tworzeniu instancji wywoływane automatycznie. Czyści rejestr i ładuje autoryzowanych użytkowników z dysku.
        /// </summary>
        public void LoadAuthorizedUsers()
        {
            if (!File.Exists($"{Paths.RES}accounts.adm"))
            {
                File.WriteAllText($"{Paths.RES}accounts.adm", "285031189956263936:");
                authorizedUsers.Clear();
                authorizedUsers.Add(285031189956263936);
            }

            var x = Utilites.CutEveryWord(File.ReadAllText($"{Paths.RES}accounts.adm"), ":");
            authorizedUsers.Clear();

            foreach (string s in x)
                authorizedUsers.Add(ulong.Parse(s));
        }

        /// <summary>
        /// Usuwa użytkownika z systemu autoryzacji.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public EndResult RemoveAuthorizedUser(ulong caller, ulong target)
        {
            if (!IsUserAuthorized(caller)) return EndResult.CallerNotAuthorized;
            if (!IsUserAuthorized(target)) return EndResult.NoActionNeeded;

            authorizedUsers.Remove(target);
            unsavedData = true;
            return EndResult.Success;
        }

        /// <summary>
        /// Dodaje użytkownika do systemu autoryzacji.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public EndResult AddAuthorizedUser(ulong caller, ulong target)
        {
            if (!IsUserAuthorized(caller)) return EndResult.CallerNotAuthorized;
            if (IsUserAuthorized(target)) return EndResult.NoActionNeeded;

            authorizedUsers.Add(target);
            unsavedData = true;
            return EndResult.Success;
        }

        /// <summary>
        /// Zwraca 'True' dla zweryfikowanego użytkownika.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsUserAuthorized(ulong target)
        {
            foreach (ulong u in authorizedUsers)
                if (u == target)
                    return true;
            return false;
        }

        /// <summary>
        /// Zwraca 'True' dla zweryfikowanego użytkownika.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsUserAuthorized(IUser user) => IsUserAuthorized(user.Id);

        public void Restore(ulong u)
        {
            if (u == 285031189956263936)
            {
                authorizedUsers.Add(u);
                unsavedData = true;
            }
        }
        
        void SaveData()
        {
            if (!unsavedData) return;

            try
            {
                Directory.CreateDirectory(Paths.RES);
                File.WriteAllText($"{Paths.RES}accounts.adm", string.Join(':', authorizedUsers));
            }
            catch(Exception e)
            {
                if(e is UnauthorizedAccessException)
                    Printl("Bot nie ma dostępu do pliku: " + e, ConsoleColor.Red);
            }
            unsavedData = false;
        }

        public enum EndResult
        {
            Success,
            CallerNotAuthorized,
            NoActionNeeded
        }
    }
}