using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Rayiah.Ekumena.Leaders;

namespace Rayiah.Ekumena
{
    public static class EkumenaInterpreter
    {
        public static IReadOnlyDictionary<PlayerCharacterType, PlayerCharacterClasses> Classes => classes;
        static Dictionary<PlayerCharacterType, PlayerCharacterClasses> classes = new Dictionary<PlayerCharacterType, PlayerCharacterClasses>();

        public static void InitInterpreter()
        {
            classes.Add(PlayerCharacterType.Farmer, new Farmer());
            classes.Add(PlayerCharacterType.Archeologist, new Archeologist());
            classes.Add(PlayerCharacterType.Architect, new Architect());
            classes.Add(PlayerCharacterType.Diplomat, new Diplomat());
            classes.Add(PlayerCharacterType.Guru, new Guru());
            classes.Add(PlayerCharacterType.Mechanic, new Mechanic());
            classes.Add(PlayerCharacterType.Miner, new Miner());
            classes.Add(PlayerCharacterType.President, new President());
            classes.Add(PlayerCharacterType.Scientist, new Scientist());
            classes.Add(PlayerCharacterType.Soldier, new Soldier());
        }
    }

    public abstract class PlayerCharacterClasses
    {
        public virtual void EveryTurn(EkumenaPlayer player) { return; }
        public virtual Dictionary<ResourceType, int> OnSuppliesAdded(Dictionary<ResourceType, int> resources) => resources;
        public virtual Dictionary<ResourceType, int> OnSuppliesUpkeep(Dictionary<ResourceType, int> resources) => resources;
        public virtual Dictionary<ResourceType, int> OnBuildSomething(Dictionary<ResourceType, int> resources) => resources;

        public abstract PlayerCharacterType Type { get; }
        public string GetClassName { get => Type.ToString(); }
    }

    public static class EkumenaGameManager
    {
        public static DateTime LastTurn { get; private set; }

        public static void NextTurn()
        {
            lock (EkumenaStorage.ActivePlayers)
            {
                foreach (ulong ply in EkumenaStorage.RegisteredPlayers.Keys)
                {
                    var player = EkumenaStorage.GetPlayer(ply);
                    var territories = EkumenaLoader.GetNormalMapPositions(EkumenaLoader.GetPlayerTerritories(ply));
                    player.SetResource(ResourceType.Happiness, 10);

                    List<Point> rareGatherRange = new List<Point>();
                    Dictionary<Point, BuildingInfo> buildings = new Dictionary<Point, BuildingInfo>();
                    Dictionary<Point, ResourceType> rareResources = new Dictionary<Point, ResourceType>();
                    Dictionary<ResourceType, int> gatheredResources = new Dictionary<ResourceType, int>();
                    foreach (MapInfoStruct p in territories)
                    {
                        if (gatheredResources.ContainsKey(ResourceType.Happiness))
                            gatheredResources[ResourceType.Happiness] = 10;
                        else
                            gatheredResources.Add(ResourceType.Happiness, 10);

                        //MainMap
                        var res = EkumenaStorage.ColorInfoToResource(p.info);
                        if (res != ResourceType.None)
                        {
                            if (gatheredResources.ContainsKey(res))
                                gatheredResources[res] += 1;
                            else
                                gatheredResources.Add(res, 1);
                        }

                        //Resources
                        res = EkumenaStorage.ColorInfoToResource(p.resType);
                        if (res != ResourceType.None)
                        {
                            if (gatheredResources.ContainsKey(res))
                                gatheredResources[res] += 1;
                            else
                                gatheredResources.Add(res, 1);
                        }

                        //RareResources
                        res = EkumenaStorage.ColorInfoToResource(p.resType);
                        if (res != ResourceType.None)
                            rareResources.Add(p.point, res);

                        //Buildings
                        if (p.buildingType != BuildingInfo.None)
                            buildings.Add(p.point, p.buildingType);
                    }

                    foreach (Point pb in buildings.Keys)
                    {
                        if (buildings[pb] == BuildingInfo.Town)
                        {
                            var points = EkumenaLoader.CircleDetection(pb, 5);
                            foreach (Point pp in points)
                            {
                                if (rareGatherRange.Contains(pp)) continue;
                                rareGatherRange.Add(pp);
                            }
                        }
                        else if (buildings[pb] == BuildingInfo.City)
                        {
                            var points = EkumenaLoader.CircleDetection(pb, 10);
                            foreach (Point pp in points)
                            {
                                if (rareGatherRange.Contains(pp)) continue;
                                rareGatherRange.Add(pp);
                            }
                        }
                        else if (buildings[pb] == BuildingInfo.Farm)
                        {
                            if (gatheredResources.ContainsKey(ResourceType.Food))
                                gatheredResources[ResourceType.Food] += 5;
                            else
                                gatheredResources.Add(ResourceType.Food, 5);
                        }
                        else if(buildings[pb] == BuildingInfo.Tower)
                        {
                            var points = EkumenaLoader.CircleDetection(pb, 15);
                            foreach (Point pp in points)
                                  player.AddIntel(pp, new IntelStruct(false, true));
                        }
                    }

                    foreach (Point p in rareGatherRange) {
                        if (rareResources.ContainsKey(p)) {
                            if (player.Intel[p].r)
                            {
                                if (gatheredResources.ContainsKey(rareResources[p]))
                                    gatheredResources[rareResources[p]] += 4;
                                else
                                    gatheredResources.Add(rareResources[p], 4);
                            }
                        }
                    }

                    var resources = player.Class.OnSuppliesAdded(gatheredResources);
                    player.AddResources(resources);
                    player.Class.EveryTurn(player);

                    // Na końcu
                    EkumenaStorage.SavePlayer(player);
                } // Koniec 'Lock'

                LastTurn = DateTime.Now;
            }
        }

        public static async Task Timer()
        {
            do
            {
                await Task.Delay(60000);
                if ((DateTime.Now - DateTime.Now).Minutes > 720)
                {
                    NextTurn();
                    break;
                }
            }
            while (true);
        }
    }

    public enum PlayerCharacterType
    { 
        Mechanic, // Zmniejsza zużycie ropy o 10%
        President, // Zwiększa produkcje hajsu o 10%
        Farmer, // Zwiększa produkcję jedzenia z pól o 10%
        Soldier, // Jeszcze bez funkcji, bo nie ma armii XDD ale będzie coś miał fajnego
        Diplomat, // Zwiększa szanse na zdobycie intela na wrogich terytoriach, nawet gdy są zamknięte granice.
        Scientist, // Zwiększa szansę na odkrycie rzadkiej technologii.
        Guru, // Zwiększa zadowolenie o 10%
        Archeologist, // Zwiększa szansę na odkrycie rzadkich złóż
        Miner, // Zwiększa o 10% produkcji copperu i irona 
        Architect // Zmniejsza koszt budowli o 5%
    }
}
