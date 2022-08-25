using Rayiah.Objects.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rayiah.Managers
{
    /// <summary>
    /// Singleton przechowujący ścieżki.
    /// </summary>
    class Paths : ManagerBase
    {
        /// <summary>
        /// Obecnie przypisana instancja (singleton).
        /// </summary>
        public static Paths instance { get; private set; } = new Paths();

        /// <summary>
        /// Ścieżka dla zbioru zasobów
        /// </summary>
        public static string RES { get; private set; } = "./Resources/";
        /// <summary>
        /// Ścieżka dla tekstur
        /// </summary>
        public static string TEXPath { get; private set; } = $"{RES}/Textures/";
        /// <summary>
        /// Ścieżka dla trybu RayiahGalaxy
        /// </summary>
        public static string GALAXIESPath { get; private set; } = $"{RES}/Galaxy/";
        /// <summary>
        /// Ścieżka dla trybu RayiahGalaxy i jego zasobów
        /// </summary>
        public static string GALRESPath { get; private set; } = $"{RES}/GalaxyResources/";
        /// <summary>
        /// Ścieżka dla trybu CardsAgainstHumanity
        /// </summary>
        public static string DECPath { get; private set; } = $"{RES}/Decks/";
        /// <summary>
        /// Ścieżka dla plików tymczasowych.
        /// </summary>
        public static string TEMPPath { get; private set; } = "./Temp/";
        /// <summary>
        /// Ścieżka dla trybu Ekumena
        /// </summary>
        public static string EKUMPath { get; private set; } = $"{RES}/Ekumena/";
        /// <summary>
        /// Ścieżka dla backupów tekstowych.
        /// </summary>
        public static string BACPath { get; private set; } = $"./Backups/";
        /// <summary>
        /// Ścieżka dla trybu AeriaStories
        /// </summary>
        public static string AERIA { get; private set; } = $"{RES}/AeriaStories/";
        /// <summary>
        /// Ścieżka dla trybu AeriaStories i jego tekstur
        /// </summary>
        public static string AERIATEX { get; private set; } = $"{AERIA}/Textures/";

        Dictionary<Type, string> paths;

        private Paths() { paths = new Dictionary<Type, string>(); }

        /// <summary>
        /// W razie potrzeby, dokona inicjalizacji obiektu. Zwraca singleton.
        /// </summary>
        /// <returns></returns>
        public static Paths CreateInstance()
        {
            if (instance != null) return instance;
            instance = new Paths();
            return instance;
        }

        /// <summary>
        /// Dodaje typ wraz ze ścieżką dla usprawnienia otrzymywania ścieżki.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="path"></param>
        public void AddTypeRecognize(Type type, string path)
        {
            if (!paths.ContainsKey(type))
            {
                paths.Add(type, path);
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Odzyskuje ścieżkę dla danego typu. Zwraca 'Unrecognized', gdy typ jest niezdefiniowany w rejestrze.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public string TypeToPath(Type type)
        {
            if (paths.TryGetValue(type, out string path))
                return path;
            return "unrecognized";
        }
    }
}
