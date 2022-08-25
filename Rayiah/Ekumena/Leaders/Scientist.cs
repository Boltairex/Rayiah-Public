using System;

namespace Rayiah.Ekumena.Leaders
{
    public class Scientist : PlayerCharacterClasses
    {
        public override PlayerCharacterType Type => PlayerCharacterType.Scientist;

        static Random rand { get; } = new Random();

        public override void EveryTurn(EkumenaPlayer player)
        {
            if (rand.Next(100) <= 5)
            {
                player.SendMessage("Twój dowódca właśnie wpadł na pomysł, który po odpowiednim zbadaniu może okazać się naprawdę korzystny...");
            }
        }
    }
}
