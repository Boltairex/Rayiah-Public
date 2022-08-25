using Rayiah.AeriaStories.Objects;
using System.Collections.Generic;

namespace Rayiah.AeriaStories
{
    public static class ItemContainer
    {
        public static List<Equipment> Helmets = new List<Equipment>();
        public static List<Equipment> Chestplates = new List<Equipment>();
        public static List<Equipment> Leggins = new List<Equipment>();
        public static List<Equipment> Boots = new List<Equipment>();
        public static List<Equipment> Weapons = new List<Equipment>();
        public static List<Equipment> SideWeapons = new List<Equipment>();

        public static Equipment GetEquipmentByIID(string IID)
        {
            var type = IIDReader.GetIIDType(IID);
            return type switch
            {
                EqType.Helmet => Helmets.Find((H) => H.info.IID == IID),
                EqType.Chestplate => Chestplates.Find((C) => C.info.IID == IID),
                EqType.Leggins => Leggins.Find((L) => L.info.IID == IID),
                EqType.Boots => Boots.Find((B) => B.info.IID == IID),
                EqType.Weapon => Weapons.Find((W) => W.info.IID == IID),
                EqType.SideWeapon => SideWeapons.Find((V) => V.info.IID == IID),
                _ => null
            };
        }

        public static Equipment GetEquipmentByName(string Name)
        {
            Equipment result;
            result = Helmets.Find((H) => H.Name == Name);
            if (result != null) return result;
            result = Chestplates.Find((C) => C.Name == Name);
            if (result != null) return result;
            result = Leggins.Find((L) => L.Name == Name);
            if (result != null) return result;
            result = Boots.Find((B) => B.Name == Name);
            if (result != null) return result;
            result = Weapons.Find((W) => W.Name == Name);
            if (result != null) return result;
            result = SideWeapons.Find((V) => V.Name == Name);
            return result;
        }
    }
}