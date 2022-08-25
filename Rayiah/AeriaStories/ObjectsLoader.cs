using Rayiah.Management;
using Rayiah.AeriaStories.Objects;
using Newtonsoft.Json;
using System.IO;
using System;

namespace Rayiah.AeriaStories
{
    public static class ObjectsLoader
    {
        public static EquipmentInfo GetConfig(string IID)
        {
            string path = $"./{Container.AERIA}/Items/";

            EqType type = IIDReader.GetIIDType(IID);
            if (type == EqType.Helmet) path += "HelmetsDir";
            else if (type == EqType.Chestplate) path += "ChestplatesDir";
            else if (type == EqType.Leggins) path += "LegginsDir";
            else if (type == EqType.Boots) path += "BootsDir";
            else if (type == EqType.Weapon) path += "WeaponsDir";
            else if (type == EqType.SideWeapon) path += "SideWeaponsDir";
            else return new EquipmentInfo();

            using (StreamReader reader = new StreamReader($"{path}/{type}.json"))
            {
                return JsonConvert.DeserializeObject<EquipmentInfo>(reader.ReadToEnd());
            }
        }

        public static EquipmentInfo Deserialize(string content) => JsonConvert.DeserializeObject<EquipmentInfo>(content);

        /// <summary>
        /// Zapisuje Equipment na dysk.
        /// </summary>
        public static bool SaveConfig(Equipment obj, bool overwrite = false)
        {
            string path = $"./{Container.AERIA}/Items/";

            if (obj.type == EqType.Helmet) path += "HelmetsDir/";
            else if (obj.type == EqType.Chestplate) path += "ChestplatesDir/";
            else if (obj.type == EqType.Leggins) path += "LegginsDir/";
            else if (obj.type == EqType.Boots) path += "BootsDir/";
            else if (obj.type == EqType.Weapon) path += "WeaponsDir/";
            else if (obj.type == EqType.SideWeapon) path += "SideWeaponsDir/";


            path += $"{obj.Name}.json";
            if (File.Exists(path) && !overwrite)
                return false;
            else
                File.WriteAllText(path, JsonConvert.SerializeObject(obj.info));
            return true;
        }
        /// <summary>
        /// Zapisuje Equipment na dysk.
        /// </summary>
        public static bool SaveConfig(EquipmentInfo obj, string name, EqType type, bool overwrite = false)
        {
            string path = $"./{Container.AERIA}/Items/";

            if (type == EqType.Helmet) path += "HelmetsDir/";
            else if (type == EqType.Chestplate) path += "ChestplatesDir/";
            else if (type == EqType.Leggins) path += "LegginsDir/";
            else if (type == EqType.Boots) path += "BootsDir/";
            else if (type == EqType.Weapon) path += "WeaponsDir/";
            else if (type == EqType.SideWeapon) path += "SideWeaponsDir/";

            path += $"{name}.json";
            if (File.Exists(path) && !overwrite)
                return false;
            else
                File.WriteAllText(path, JsonConvert.SerializeObject(obj));
            return true;
        }
    }
}