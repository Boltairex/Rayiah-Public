using Discord.WebSocket;
using Rayiah.Objects.Abstracts;
using Rayiah.Objects.Interfaces;
using Rayiah.Objects.Structs;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rayiah.Managers
{
    class RolesBackup : ManagerBase, IPath
    {
        public static RolesBackup instance { get; private set; } = new RolesBackup();

        public override string Name => GetType().Name;

        string path => Paths.RES;
        ulong focusedGuild = 0;
        ulong focusedUser = 0;
        int focusedUserIndex = 0;

        bool displayErrors = false;

        List<UserRolesData> cache = new List<UserRolesData>();
        private readonly object inUse = new object();

        public RolesBackup()
        {
            Paths.CreateInstance().AddTypeRecognize(typeof(RolesBackup), $"{path}RolesBackup/");
            AppDomain.CurrentDomain.ProcessExit += OnExit;
        }

        private void OnExit(object sender, EventArgs e)
        {
            ForceCacheSave();
        }

        /// <summary>
        /// Create instance if singleton doesn't exist.
        /// </summary>
        /// <returns></returns>
        public static RolesBackup CreateInstance()
        {
            if (instance != null) return instance;
            instance = new RolesBackup();
            return instance;
        }

        /// <summary>
        /// Write to disk. Handles exceptions.
        /// </summary>
        public void SaveData(List<UserRolesData> data, ulong guild, bool force = false)
        {
            if (data is null || data.Count == 0) return;
            try
            {
                //Print("Zapisuję " + data.Count, "SaveData");
                Tools.Utilites.SaveJSON(data, $"{GetPath()}{guild}.json");
                //Print("Zapisałem", "SaveData");
            }
            catch (Exception e) { Console.WriteLine(e); }

            if (force)
            {
                ForceCacheSave();
                ChangeGuild(guild);
                cache = data;
            }
        }

        /// <summary>
        /// Copy of memory data.
        /// </summary>
        public List<UserRolesData> GetData(ulong guild)
        {
            List<UserRolesData> copy = new List<UserRolesData>();
            if (!LoadData(guild)) return copy;
            foreach (UserRolesData ob in cache)
                copy.Add(ob);

            return copy;
        }

        /// <summary>
        /// Updates user roles. 
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public bool UpdateUser(ulong guild, ulong user, IReadOnlyCollection<SocketRole> roles)
        {
            if (roles.Count == 0) return false;
            bool empty = !LoadData(guild);
            if (empty || !ContainsUser(guild, user))
            {
                cache.Add(new UserRolesData(user, roles.GetEnumerator()));
                //Print("Dodałem. " + cache.Count);
            }
            else
            {
                foreach (UserRolesData ob in cache)
                {
                    if (ob.user == user)
                    {
                        ob.SetRoles(roles.GetEnumerator());
                        break;
                    }
                }
            }
            return true;
        }

        public bool ContainsUser(ulong guild, ulong user)
        {
            if (!LoadData(guild)) return false;
            if (focusedUser == user) return true;
            //Print("Mam " + cache.Count + " dzbanów");
            for (int x = 0; x < cache.Count; x++)
            {
                //Print(cache[x].user);
                if (cache[x].user == user)
                {
                    focusedUser = user;
                    focusedUserIndex = x;
                    return true;
                }
            }
            return false;
        }

        public ulong[] GetUserPermisions(ulong guild, ulong user)
        {
            if (!LoadData(guild)) return new ulong[0];
            if (focusedUser == user)
                return cache[focusedUserIndex].roles;
            else
            {
                for (int x = 0; x < cache.Count; x++)
                {
                    if (cache[x].user == user)
                    {
                        focusedUser = user;
                        focusedUserIndex = x;
                        return cache[x].roles;
                    }
                }
            }
            return new ulong[0];
        }

        public string GetPath() => Paths.instance.TypeToPath(typeof(RolesBackup));

        void ForceCacheSave()
        {
            //Print("Cache jest nullowany? " + (cache is null));
            if (focusedGuild == 0 || cache is null || cache.Count == 0) return;
            try
            {
                //Print("Bruh");
                Tools.Utilites.SaveJSON(cache, $"{GetPath()}{focusedGuild}.json");
            }
            catch (Exception e)
            {
                Printl(e);
            }
        }

        /// <summary>
        /// Load from disk to memory
        /// </summary>
        /// <param name="guild"></param>
        bool LoadData(ulong guild)
        {
            try
            {
                //Print(guild + " | " + focusedGuild);
                if (guild != focusedGuild)
                {
                    ForceCacheSave();
                    ChangeGuild(guild);
                    //Print("Wczytuję gildię!");
                    try
                    {
                        var val = Tools.Utilites.LoadJSON<List<UserRolesData>>($"{GetPath()}{guild}.json");
                        if (cache is null)
                            cache = val;
                    }
                    catch (Exception e)
                    {
                        if (displayErrors)
                        {
                            if (e is FileNotFoundException)
                                Printl("Plik nie istnieje");
                            else
                                Printl(e);
                        }
                        if (cache is null)
                            cache = new List<UserRolesData>();
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e) { Printl(e); return false; }
        }

        void ChangeGuild(ulong guild)
        {
            //Print("Zmieniam gildie na " + guild);
            focusedGuild = guild;
            focusedUser = 0;
            focusedUserIndex = 0;
            if (cache is null || cache.Count == 0)
                cache = new List<UserRolesData>();
            else
                cache.Clear();
        }
    }
}