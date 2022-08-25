using System;
using System.Drawing;
using System.Collections.Generic;

namespace Rayiah.Ekumena.Leaders
{
    public class Archeologist : PlayerCharacterClasses
    {
        static Random rand { get; } = new Random();
        public override PlayerCharacterType Type => PlayerCharacterType.Archeologist;

        public override void EveryTurn(EkumenaPlayer player)
        {
            if (rand.Next(100) <= 10)
            {
                int i = rand.Next(2, 5);
                var poses = EkumenaLoader.GetNormalMapPositions(EkumenaLoader.GetPlayerTerritories(player.User.Id));
                Dictionary<Point, ColorInfo> info = new Dictionary<Point, ColorInfo>();
                foreach (MapInfoStruct str in poses)
                {
                    if (str.rareType != ColorInfo.NotUsable)
                        info.Add(str.point, str.rareType);
                }

                foreach (Point p in info.Keys) {
                    if (player.Intel[p].r) {
                        info.Remove(p);
                    }
                }

                if (info.Keys.Count <= i)
                {
                    foreach (Point p in info.Keys)
                        player.AddIntel(p, new IntelStruct(true, true));

                    player.SendMessage("Twojemu dowódcy udało się znaleźć ukryte surowce na twoich terytoriach... Wygląda też na to, że nie ma więcej surowców na twoim terytorium.");
                }
                else if (info.Keys.Count == 0)
                {
                    player.SendMessage("Twój dowódca poszedł na poszukiwania złóż, ale niestety nie znalazł żadnego na **zajętym** przez ciebie terytorium. Rozwijaj się, by mógł następnym razem się wykazać.");
                }
                else
                {
                    int x = 0;
                    foreach (Point p in info.Keys) {
                        player.AddIntel(p, new IntelStruct(true, true));
                        x++;
                        if (x > i)
                            break;
                    }

                    player.SendMessage("Twojemu dowódcy udało się znaleźć ukryte surowce na twoich terytoriach... Może to być miedź, żelazo... albo ropa.");
                }
            }
        }
    }
}
