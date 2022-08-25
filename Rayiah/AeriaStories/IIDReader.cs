using Rayiah.AeriaStories.Objects;
using System.IO;
using Rayiah.Management;
using System;
using Newtonsoft.Json;

namespace Rayiah.AeriaStories
{
    // "H/C/L/B/W/V..."
    // Czteroczłonowe Item ID

    public static class IIDReader
    {
        public static EqType GetIIDType(string IID)
        {
            if (IID[0] == 'H')
                return EqType.Helmet;
            else if (IID[0] == 'C')
                return EqType.Chestplate;
            else if (IID[0] == 'L')
                return EqType.Leggins;
            else if (IID[0] == 'B')
                return EqType.Boots;
            else if (IID[0] == 'W')
                return EqType.SideWeapon;
            else if (IID[0] == 'V')
                return EqType.Weapon;
            else
                throw new Exception("Bad format.");
        }

        public static bool isIDTaken(string IID)
        {
            string path = $"{Container.AERIA}Items/";
            if (IID[0] == 'H') path += "HelmetsDir/";
            else if (IID[0] == 'C') path += "ChestplatesDir/";
            else if (IID[0] == 'L') path += "LegginsDir/";
            else if (IID[0] == 'B') path += "BootsDir/";
            else if (IID[0] == 'W') path += "WeaponsDir/";
            else if (IID[0] == 'V') path += "SideWeaponsDir/";
            else throw new Exception("Bad format.");

            DirectoryInfo info = new DirectoryInfo(path);
            foreach (FileInfo i in info.GetFiles()) {
                using (StreamReader reader = new StreamReader(i.FullName))
                {
                    if (JsonConvert.DeserializeObject<EquipmentInfo>(reader.ReadToEnd()).IID == IID)
                        return true;
                }
            }
            return false;
        }
    }
}
