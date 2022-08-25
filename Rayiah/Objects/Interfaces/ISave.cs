using Rayiah.Managers;
using Rayiah.Objects.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Rayiah.Objects.Interfaces
{
    /// <summary>
    /// Interface for easy save'n load of values tagged by <see cref="SaveAttribute"/>.
    /// </summary>
    public interface ISave
    {
        /// <summary>
        /// Change before initializing any class with <see cref="ISave"/>: low risk of not working properly after.
        /// <para></para>
        /// By default: <c>./LoadData/</c>
        /// </summary>
        public static string FilePath = "./LoadData/";

        /// <summary>
        /// False, when directory wasn't exist - skipping loading.
        /// </summary>
        public static bool Load { get; private set; } = true;

        /// <summary>
        /// Disables any nortification print.
        /// </summary>
        public static bool HideNortifications = false;

        /// <summary>
        /// Enables auto-save when <see cref="AppDomain.CurrentDomain.ProcessExit"/> occurs. Enabled by default.
        /// </summary>
        public static bool AutoSave = true;

        static Dictionary<int, object> toSave { get; } = new Dictionary<int, object>();

        static ISave()
        {
            AppDomain.CurrentDomain.ProcessExit += SaveClassesValues;
        }

        /// <summary>
        /// Checks that directory exist.
        /// </summary>
        /// <returns></returns>
        public static bool PrepareDir()
        {
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
                Load = false;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Add object and check it contains any <see cref="SaveAttribute"/>, either skips it.
        /// </summary>
        /// <param name="obj"></param>
        public static void AddObject(object obj)
        {
            bool containAttributes = false;
            foreach (FieldInfo f in obj.GetType().GetFields())
            {
                var attr = f.GetCustomAttribute(typeof(SaveAttribute));
                if (attr is null) continue;
                if (!f.IsPublic) { Console.WriteLine($"Skipping '{f.Name}': needs to be public!"); continue; }

                containAttributes = true;
                break;
            }

            if (containAttributes && !toSave.ContainsKey(obj.GetHashCode()))
                toSave.Add(obj.GetHashCode(), obj);
        }

        /// <summary>
        /// Need to be called manually from somewhere. Proceeds to load values saved on disk.
        /// </summary>
        public static void LoadClassesValues(bool force = false)
        {
            PrepareDir();
            if (!Load && !force)
            {
                Console.WriteLine("Skipping load of values: no directory detected.");
                return;
            }

            foreach (object obj in toSave.Values)
            {
                SaveParser.instance.GatherValues(obj.GetType());
            }
        }

        /// <summary>
        /// Manually called save of all gathered classes.
        /// </summary>
        public static void SaveClassesValues()
        {
            foreach (object obj in toSave.Values)
            {
                SaveParser.instance.SaveValues(obj.GetType());
            }
        }

        /// <summary>
        /// Called by <see cref="AppDomain.CurrentDomain.ProcessExit"/>.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="__"></param>
        public static void SaveClassesValues(object _, EventArgs __) => SaveClassesValues();
    }
}
