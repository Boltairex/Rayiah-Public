using System;
using System.Drawing;
using Discord;

namespace Rayiah.AeriaStories.Objects
{
    public abstract class Equipment
    {
        protected Equipment(EquipmentInfo info)
        {
            this.info = info;
            //LoadIMG();
        }

        public abstract EqType type { get; }
        public string Name { get; protected set; }
        public EquipmentInfo info { get; protected set; }
        /*protected Image img;

        public void LoadIMG()
        {
            try
            {
                if(info.imgPath != null)
                    img = Image.FromFile(info.imgPath);
            }
            catch (Exception E) { Console.WriteLine(E); }
        }*/
    }

    public sealed class Helmet : Equipment
    {
        public Helmet(EquipmentInfo info) : base(info) { return; }

        public override EqType type => EqType.Helmet;
    }

    public sealed class Chestplate : Equipment
    {
        public Chestplate(EquipmentInfo info) : base(info) { return; }

        public override EqType type => EqType.Chestplate;
    }

    public sealed class Leggins : Equipment
    {
        public Leggins(EquipmentInfo info) : base(info) { return; }

        public override EqType type => EqType.Leggins;
    }

    public sealed class Boots : Equipment
    {
        public Boots(EquipmentInfo info) : base(info) { return; }

        public override EqType type => EqType.Boots;
    }

    public sealed class Weapon : Equipment
    {
        public Weapon(EquipmentInfo info) : base(info) { return; }

        public override EqType type => EqType.Weapon;
    }

    public sealed class SideWeapon : Equipment
    {
        public SideWeapon(EquipmentInfo info) : base(info) { return; }

        public override EqType type => EqType.SideWeapon;
    }

    public struct EquipmentInfo // To jest format load'u do pamięci.
    {
        public string imgPath;
        public string desc;
        public string IID; // Po tym można rozpoznać typ, oraz nazwę przedmiotu. Identyfikator wirtualny
        public string[] modifiers;
        public int power;
        //potem jakieś abilitki dam.
    }

    public struct EqInstance
    {
        public string IID;
        public int lvl;
    }

    public enum EqType
    {
        Helmet,
        Chestplate,
        Leggins,
        Boots,
        Weapon,
        SideWeapon
    }
}
