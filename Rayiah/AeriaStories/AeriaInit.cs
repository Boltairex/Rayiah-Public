using Discord.Commands;
using Rayiah.AeriaStories.Objects;
using Rayiah.Management;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rayiah.AeriaStories
{
    public static class Init
    {
        public delegate void Cycle();
        public static event Cycle EveryCycle;

        public delegate void DeleteCharacter(ulong u, SocketCommandContext context);
        static event DeleteCharacter ConfirmDelete;

        public static string SafetyCheck()
        {
            try
            {
                Directory.CreateDirectory($"{Container.AERIA}/Characters/");
                Directory.CreateDirectory($"{Container.AERIA}Items/HelmetsDir/");
                Directory.CreateDirectory($"{Container.AERIA}Items/ChestplatesDir/");
                Directory.CreateDirectory($"{Container.AERIA}Items/LegginsDir/");
                Directory.CreateDirectory($"{Container.AERIA}Items/BootsDir/");
                Directory.CreateDirectory($"{Container.AERIA}Items/WeaponsDir/");
                Directory.CreateDirectory($"{Container.AERIA}Items/SideWeaponsDir/");
                return "Inicjalizacja resource'ów Aeria Stories zakończona...";
            }
            catch (Exception E) { return E.Message; }
        }

        public static void ConfirmDeleteV(ulong u, SocketCommandContext context) => ConfirmDelete?.Invoke(u, context);

        public static string Resources()
        {
            try
            {
                ConfirmDelete += CharacterSerializer.RequestCheck;
                DirectoryInfo[] info = new DirectoryInfo($"{Container.AERIA}Items/").GetDirectories();

                foreach (DirectoryInfo I in info) {
                    if (I.Name == "HelmetsDir") {
                        foreach (FileInfo F in I.GetFiles()) {
                            using (StreamReader reader = new StreamReader(F.FullName))
                            {
                                ItemContainer.Helmets.Add(new Helmet(ObjectsLoader.Deserialize(reader.ReadToEnd())));
                            }
                        }
                    }
                    else if (I.Name == "ChestplatesDir") {
                        foreach (FileInfo F in I.GetFiles()) {
                            using (StreamReader reader = new StreamReader(F.FullName))
                            {
                                ItemContainer.Helmets.Add(new Chestplate(ObjectsLoader.Deserialize(reader.ReadToEnd())));
                            }
                        }
                    }

                    else if (I.Name == "LegginsDir") {
                        foreach (FileInfo F in I.GetFiles()) {
                            using (StreamReader reader = new StreamReader(F.FullName))
                            {
                                ItemContainer.Helmets.Add(new Leggins(ObjectsLoader.Deserialize(reader.ReadToEnd())));
                            }
                        }
                    }
                    else if (I.Name == "BootsDir") {
                        foreach (FileInfo F in I.GetFiles()) {
                            using (StreamReader reader = new StreamReader(F.FullName))
                            {
                                ItemContainer.Helmets.Add(new Boots(ObjectsLoader.Deserialize(reader.ReadToEnd())));
                            }
                        }
                    }
                    else if (I.Name == "WeaponsDir") {
                        foreach (FileInfo F in I.GetFiles()) {
                            using (StreamReader reader = new StreamReader(F.FullName))
                            {
                                ItemContainer.Helmets.Add(new Weapon(ObjectsLoader.Deserialize(reader.ReadToEnd())));
                            }
                        }
                    }
                    else if (I.Name == "SideWeaponsDir") {
                        foreach (FileInfo F in I.GetFiles()) {
                            using (StreamReader reader = new StreamReader(F.FullName))
                            {
                                ItemContainer.Helmets.Add(new SideWeapon(ObjectsLoader.Deserialize(reader.ReadToEnd())));
                            }
                        }
                    }
                }

                return "Załadowano resource'y Aeria Stories.";
            }
            catch (Exception E) { return E.Message; }
        }

        public static void LateInit()
        {
            // podpisywanie pod eventy

            EveryCycle += Storage.Cycle;

            // na końcu
            Task.Run(Mechanism).GetAwaiter();
        }

        public static async Task Mechanism()
        {
        s:
            EveryCycle.Invoke();
            await Task.Delay(60000);
            goto s;
        }
    }
}