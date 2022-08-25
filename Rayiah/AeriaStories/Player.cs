using System;
using Discord;
using static Rayiah.Tools.RayiahUtilites;

namespace Rayiah.AeriaStories.Objects
{
    public class Character
    {
        public Character(CharacterInstance instance)
        {
            this.instance = instance;
            timeLeft = DateTime.Now;
        }

        public IUser owner;

        DateTime timeLeft;
        CharacterInstance instance;

        public void ResetTimer() => timeLeft = DateTime.Now;

        public bool CheckTimer()
        {
            if (DateTime.Now.Subtract(timeLeft).TotalMinutes > 1)
            {
                OnDestroy();
                return true;
            }
            return false;
        }

        public void OnDestroy()
        {
            owner.SendMessageAsync("Sesja upłynęła.");
        }
    }

    public struct CharacterInstance
    {
        public string name;
        public string age;
        public string desc;
        public string url;
        public string color;
        public int power;
        public EqInstance[] eq;
    }
}
