using Discord;
using Rayiah.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static Rayiah.Management.Container;

namespace Rayiah.Ekumena
{
    public class EkumenaPlayer
    {
        #region Properties
        public IReadOnlyDictionary<Point, IntelStruct> Intel => intel;
        public IReadOnlyDictionary<ResourceType, int> Resources { get => resources; }
        public System.Drawing.Color Color { get => color; }
        #endregion
        #region Fields

        public readonly Point Capital;
        public Dictionary<Point, IntelStruct> intel;
        public PlayerCharacterClasses Class { get; private set; }
        public IUser User { get; private set; }
        public string Name;
        public string Desc;

        System.Drawing.Color color;
        Dictionary<ResourceType, int> resources;

        #endregion
        public EkumenaPlayer(EkumenaPlayerStruct info)
        {
            User = Program.instance.client.GetUser(info.User);
            if (User is null) throw new Exception("Failed to load User. Info: " + info.User);
            resources = info.Resources;
            intel = info.Intel;
            Class = EkumenaInterpreter.Classes[info.Class];
            color = EkumenaStorage.RegisteredPlayers[User.Id];
            Capital = info.Capital;
        }

        public void SendMessage(string message) => User.SendMessageAsync(message);

        public void SaveToJSON()
        {
            EkumenaPlayerStruct str = new EkumenaPlayerStruct(this.User.Id, Name, Desc, intel, resources, Class.Type, Capital);
            RayiahUtilites.SaveJSON(str, EKUMPath + "Players/" + User.Id + ".json");
        }

        public void AddResources(ResourceType type, int amount)
        {
            if (resources.ContainsKey(type))
                resources[type] += amount;
            else
                resources.Add(type, amount);
        }

        public void AddResources(Dictionary<ResourceType, int> resources)
        {
            foreach (ResourceType res in resources.Keys)
            {
                if (this.resources.ContainsKey(res))
                    this.resources[res] += resources[res];
                else
                    this.resources.Add(res, resources[res]);
            }
        }

        public void SetResource(ResourceType type, int i)
        {
            if (resources.ContainsKey(type))
                resources[type] = i;
            else
                resources.Add(type, i);
        }

        // Random
        public void AddIntel(Point pos, IntelStruct type, int range, int count)
        {
            List<Point> points = new List<Point>();
            var rand = new Random();
            for (int x = 0; x < count; x++)
            {
                int safeCount = 0;
                int i = pos.X, j = pos.Y;
                do
                {
                    safeCount++;
                    if (safeCount > 9)
                        break;
                    i = rand.Next(pos.X - range, pos.X + range);
                    i = i < 0 ? 0 : i > 599 ? 599 : i;

                    j = rand.Next(pos.Y - range, pos.X + range);
                    j = j < 0 ? 0 : j > 599 ? 599 : j;
                }
                while (Intel[new Point(i,j)] == type);
                points.Add(new Point(i, j));
            }

            List<IntelStruct> structs = new List<IntelStruct>();
            foreach (Point _ in points)
                structs.Add(type);

            AddIntel(points.ToArray(), structs.ToArray());
        }

        // Circle
        public void AddIntel(Point pos, IntelStruct type, int range)
        {
            var points = EkumenaLoader.CircleDetection(pos, range);
            List<IntelStruct> structs = new List<IntelStruct>();
            foreach (Point _ in points)
                structs.Add(type);
            AddIntel(points, structs.ToArray());
        }

        // Point
        public void AddIntel(Point pos, IntelStruct type)
        {
            if (pos.X < 0) pos.X = 0;
            else if (pos.X > 599) pos.X = 599;

            if (pos.Y < 0) pos.Y = 0;
            else if (pos.Y > 599) pos.Y = 599;

            AddIntel(new Point[1] { pos }, new IntelStruct[1] { type });
        }

        /// <param name="positions"></param>
        /// <param name="types"></param>
        void AddIntel(Point[] positions, IntelStruct[] types)
        {
            for (int x = 0; x < positions.Length; x++)
                if (Intel.ContainsKey(positions[x]))
                    intel[positions[x]] += types[x];
                else
                    intel.Add(positions[x], types[x]);
        }
    }

    public struct EkumenaPlayerStruct
    {
        public ulong User;
        public string Name;
        public string Desc;
        public Point Capital;
        public Dictionary<Point, IntelStruct> Intel;
        public Dictionary<ResourceType, int> Resources;
        public PlayerCharacterType Class;

        public EkumenaPlayerStruct(ulong user, string name, string desc, Dictionary<Point, IntelStruct> intel, Dictionary<ResourceType, int> resources, PlayerCharacterType pclass, Point capital)
        {
            User = user;
            Intel = intel;
            Resources = resources;
            Class = pclass;
            Name = name;
            Desc = desc;
            Capital = capital;
        }
    }

    public struct IntelStruct
    {
        public bool r; // Ukryte surowce
        public bool o; // Terytoria

        public IntelStruct(bool resource = false, bool others = false)
        {
            this.r = resource;
            this.o = others;
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(IntelStruct);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(r, o);
        }

        public static IntelStruct operator +(IntelStruct str1, IntelStruct str2)
        {
            str1.r = str1.r ? true : str2.r;
            str1.o = str1.o ? true : str2.o;

            return str1;
        }

        public static bool operator !=(IntelStruct str1, IntelStruct str2)
        {
            return str1.r != str2.r || str1.o != str2.o;
        }

        public static bool operator ==(IntelStruct str1, IntelStruct str2)
        {
            return str1.r == str2.r && str1.o == str2.o;
        }
    }

    public static class EkumenaStorage
    {
        public static IReadOnlyDictionary<ulong, System.Drawing.Color> RegisteredPlayers { get => registeredPlayers; }
        public static IReadOnlyList<EkumenaPlayer> ActivePlayers { get => activePlayers; }

        static Dictionary<ulong, System.Drawing.Color> registeredPlayers = new Dictionary<ulong, System.Drawing.Color>();
        static List<EkumenaPlayer> activePlayers = new List<EkumenaPlayer>();

        static EkumenaStorage()
        {
            AppDomain.CurrentDomain.ProcessExit += ProcessExit;
        }

        public static void InitStorage()
        {
            if (File.Exists(EKUMPath + "Registry/PlayersColors.json"))
            {
                RegisteredPlayersStruct[] rps;
                rps = RayiahUtilites.LoadJSON<RegisteredPlayersStruct[]>(EKUMPath + "Registry/PlayersColors.json");

                foreach (RegisteredPlayersStruct str in rps)
                    registeredPlayers.Add(str.user, str.color);
            }
        }

        public static ResourceType ColorInfoToResource(ColorInfo info)
        {
            return info switch
            {
                ColorInfo.Copper => ResourceType.Copper,
                ColorInfo.NotUsable => ResourceType.None,
                ColorInfo.StartPos => ResourceType.None,
                ColorInfo.DeepWater => ResourceType.Food,
                ColorInfo.Water => ResourceType.Food,
                ColorInfo.Lake => ResourceType.Food,
                ColorInfo.Snow => ResourceType.None,
                ColorInfo.Sand => ResourceType.Happiness,
                ColorInfo.Forest => ResourceType.BuildingResource,
                ColorInfo.Mountain => ResourceType.BuildingResource,
                ColorInfo.Iron => ResourceType.Metal,
                ColorInfo.Oil => ResourceType.Oil,
                _ => ResourceType.None,
            };
        }

        public static ushort ColorSum(System.Drawing.Color color) => (ushort)(color.R + color.G + color.B);

        public static (bool, ColorInfo) ColorRecognizer(System.Drawing.Color color)
        {
            ColorInfo info = (ColorInfo)(color.R + color.G + color.B);
            return (!int.TryParse(info.ToString(), out int _), info);
        }

        public static void AddPlayer(EkumenaPlayer player)
        {
            if (activePlayers.Contains(player)) return;
            activePlayers.Add(player);
        }

        /// <summary>
        /// Bezpieczne usunięcie gracza z pamięci na dysk.
        /// </summary>
        /// <param name="player"></param>
        public static void RemovePlayer(EkumenaPlayer player)
        {
            player.SaveToJSON();
            activePlayers.Remove(player);
        }

        public static EkumenaPlayer GetPlayer(ulong u)
        {
            if (IsPlayerActive(u)) // Gdy jest w pamięci
            {
                foreach (EkumenaPlayer EP in activePlayers)
                    if (EP.User.Id == u)
                        return EP;
            }
            // Gdy nie ma go w pamięci
            return LoadPlayer(u);
        }

        public static void SavePlayer(EkumenaPlayer player)
        {
            lock (activePlayers)
            {
                if (IsPlayerActive(player.User.Id)) // Gdy jest w pamięci
                {
                    for (int x = 0; x < activePlayers.Count; x++)
                        if (activePlayers[x].User.Id == activePlayers[x].User.Id)
                        {
                            activePlayers[x] = player;
                            activePlayers[x].SaveToJSON();
                        }
                }
                // Gdy nie ma go w pamięci
                player.SaveToJSON();
            }
        }

        public static bool IsPlayerActive(ulong u)
        {
            foreach (EkumenaPlayer EP in activePlayers)
                if (EP.User.Id == u)
                    return true;

            return false;
        }

        public static bool RegisteredPlayersSafeCheck(System.Drawing.Color color)
        {
            ushort colorSum = ColorSum(color);
            foreach (System.Drawing.Color c in registeredPlayers.Values)
                if (ColorSum(c) == colorSum)
                    return false;

            foreach (ushort c in EkumenaLoader.TakenColors)
                if (c == colorSum)
                    return false;

            return true;
        }

        public static bool RegisterPlayer(ulong u, System.Drawing.Color color)
        {
            if (registeredPlayers.ContainsKey(u)) return false;
            if (EkumenaLoader.TakenColors.Contains(ColorSum(color))) return false;
            if (!RegisteredPlayersSafeCheck(color)) return false;

            registeredPlayers.Add(u, color);
            return true;
        }

        //Z dysku
        static EkumenaPlayer LoadPlayer(ulong u)
        {
            if (!File.Exists(EKUMPath + "Players/" + u + ".json")) return null;
            var ply = RayiahUtilites.LoadJSON<EkumenaPlayerStruct>(EKUMPath + "Players/" + u + ".json");
            return new EkumenaPlayer(ply);
        }

        static void ProcessExit(object sender, EventArgs e)
        {
            List<RegisteredPlayersStruct> structs = new List<RegisteredPlayersStruct>();
            foreach (ulong u in RegisteredPlayers.Keys)
            {
                var color = RegisteredPlayers[u];
                structs.Add(new RegisteredPlayersStruct(u, color));
            }

            RayiahUtilites.SaveJSON(structs, EKUMPath + "Registry/PlayersColors.json");

            foreach (EkumenaPlayer p in activePlayers)
                RemovePlayer(p);
        }
    }

    public static class EkumenaLoader
    {
        static bool mainMap_dirtyBits = false;
        static bool resources_dirtyBits = false;
        static bool rareResources_dirtyBits = false;
        static bool buildings_dirtyBits = false;
        static bool territories_dirtyBits = false;

        static Bitmap mainMap;
        static Bitmap resources;
        static Bitmap rareResources;
        static Bitmap buildings;
        static Bitmap territories;

        static System.Drawing.Color bridgeColor = System.Drawing.Color.FromArgb(123, 62, 5);
        static System.Drawing.Color townColor = System.Drawing.Color.FromArgb(255, 150, 150);
        static System.Drawing.Color cityColor = System.Drawing.Color.FromArgb(255, 95, 95);
        static System.Drawing.Color towerColor = System.Drawing.Color.FromArgb(15, 35, 90);
        static System.Drawing.Color farmColor = System.Drawing.Color.FromArgb(120, 150, 100);

        public static Point[] SpawnPoints { get; private set; }

        public static ushort[] TakenColors = new ushort[17] { 0, 486, 250, 440, 322, 765, 493, 138, 429, 346, 150, 54, 190, 550, 455, 140, 370 };

        static EkumenaLoader()
        {
            AppDomain.CurrentDomain.ProcessExit += ProcessExit;
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            try
            {
                lock (mainMap)
                {
                    if (mainMap_dirtyBits)
                        mainMap.Save(EKUMPath + "MapLayers/Main.png");
                }
                lock (resources)
                {
                    if (resources_dirtyBits)
                        resources.Save(EKUMPath + "MapLayers/Resources.png");
                }
                lock (rareResources)
                {
                    if (rareResources_dirtyBits)
                        rareResources.Save(EKUMPath + "MapLayers/SpecialResources.png");
                }
                lock (buildings)
                {
                    if (buildings_dirtyBits)
                        buildings.Save(EKUMPath + "MapLayers/Buildings.png");
                }
                lock (territories)
                {
                    if (territories_dirtyBits)
                        territories.Save(EKUMPath + "MapLayers/Territories.png");
                }
            }catch(Exception err) { Console.WriteLine(err); }
        }
        public static bool IsPointSafe(Point point) => point.X >= 0 && point.X <= 599 && point.Y >= 0 && point.Y <= 599;

        public static System.Drawing.Color MainMapGetPixel(Point point)
        {
            if (IsPointSafe(point))
                return mainMap.GetPixel(point.X, point.Y);
            return System.Drawing.Color.Transparent;
        }


        /// <summary>
        /// Służy do sprawdzania, czy punkt jest bezpieczny dla funkcji 'GatherMapPixels', oraz do operacji na tablicach.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Point ProbePoint(Point center, Size size)
        {
            int xProbing = center.X - size.Width / 2 + 1;
            if (xProbing < 0) // Sprawdza, czy punkt X nie wykroczy poza tablicę
                center.X += xProbing; // Przesuwa punkt X, gdy przekroczy

            int yProbing = center.Y - size.Height / 2 + 1;
            if (yProbing < 0) // Sprawdza, czy punkt Y nie wykroczy poza tablicę
                center.Y += yProbing; // Przesuwa punkt Y, gdy przekroczy

            return center;
        }

        /// <summary>
        /// Zbieranie kolorów z mapy według <see cref="GatherMapMode"/>. Posiada zabezpieczenia przed niepoprawnymi punktami.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        static (Point[], System.Drawing.Color[]) GatherMapPixels(Point center, Size size, GatherMapMode mode)
        {
            center = ProbePoint(center, size);

            List<System.Drawing.Color> colors = new List<System.Drawing.Color>(); // Magazyn na kolory
            List<Point> points = new List<Point>(); // Magazyn na koordynaty dla kolorów

            for (int x = center.X - (size.Width / 2) + 1; x < center.X + (size.Width / 2); x++) {
                for (int y = center.Y - (size.Height / 2) + 1; y < center.Y + (size.Height / 2); y++)
                {
                    Point p = new Point(x, y); // Utworzenie structa ze sprawdzanych koordynatów.
                    if (!IsPointSafe(p)) continue; // Gdy Punkt wychodzi poza tablicę, pomija dalsze operacje.
                    points.Add(p);

                    switch (mode) // Sprawdza tryb zbierania.
                    {
                        case GatherMapMode.MainMap: // Gdy tryb zbierania ustawiony jest na MainMap
                            colors.Add(mainMap.GetPixel(x, y)); 
                            break;

                        case GatherMapMode.Resources: // Gdy tryb zbierania ustawiony jest na Resources
                            colors.Add(resources.GetPixel(x, y));
                            break;

                        case GatherMapMode.RareResources: // Gdy tryb zbierania ustawiony jest na RareResources
                            colors.Add(rareResources.GetPixel(x, y));
                            break;

                        case GatherMapMode.Territories: // Gdy tryb zbierania ustawiony jest na Territories
                            colors.Add(territories.GetPixel(x, y));
                            break;

                        case GatherMapMode.Buildings: // Gdy tryb zbierania ustawiony jest na Buildings
                            colors.Add(buildings.GetPixel(x, y));
                            break;
                    }
                }
            }
            return (points.ToArray(), colors.ToArray()); // Oddanie wyników w postaci statycznych arrayów.
        }

        public static (Point[], System.Drawing.Color[]) MainMapGetPixels(Point center, Size size)
        {
            return GatherMapPixels(center, size, GatherMapMode.MainMap);
        }

        public static void MainMapSetPixel(Point p, System.Drawing.Color c)
        {
            if (!IsPointSafe(p)) return;
            mainMap.SetPixel(p.X, p.Y, c);
            mainMap_dirtyBits = true;
        }

        public static System.Drawing.Color ResourceGetPixel(Point point)
        {
            if (IsPointSafe(point))
                return resources.GetPixel(point.X, point.Y);
            return System.Drawing.Color.Transparent;
        }

        public static (Point[], System.Drawing.Color[]) ResourceGetPixels(Point center, Size size)
        {
            return GatherMapPixels(center, size, GatherMapMode.Resources);
        }

        public static void ResourcesSetPixel(Point p, System.Drawing.Color c)
        {
            if (!IsPointSafe(p)) return;
            resources.SetPixel(p.X, p.Y, c);
            resources_dirtyBits = true;
        }

        public static System.Drawing.Color RareResourcesGetPixel(Point point)
        {
            if (IsPointSafe(point))
                return rareResources.GetPixel(point.X, point.Y);
            return System.Drawing.Color.Transparent;
        }

        public static (Point[], System.Drawing.Color[]) RareResourcesGetPixels(Point center, Size size)
        {
            return GatherMapPixels(center, size, GatherMapMode.RareResources);
        }

        public static void RareResourcesSetPixel(Point p, System.Drawing.Color c)
        {
            if (!IsPointSafe(p)) return;
            rareResources.SetPixel(p.X, p.Y, c);
            rareResources_dirtyBits = true;
        }

        public static System.Drawing.Color BuildingsGetPixel(Point point)
        {
            if (IsPointSafe(point))
                return buildings.GetPixel(point.X, point.Y);
            return System.Drawing.Color.Transparent;
        }

        public static (Point[], System.Drawing.Color[]) BuildingsGetPixels(Point center, Size size)
        {
            return GatherMapPixels(center, size, GatherMapMode.Buildings);
        }

        public static void BuildingsSetPixel(Point p, System.Drawing.Color c)
        {
            if (!IsPointSafe(p)) return;
            buildings.SetPixel(p.X, p.Y, c);
            buildings_dirtyBits = true;
        }

        public static System.Drawing.Color TerritoriesGetPixel(Point point)
        {
            if (IsPointSafe(point))
                return territories.GetPixel(point.X, point.Y);
            return System.Drawing.Color.Transparent;
        }

        public static (Point[], System.Drawing.Color[]) TerritoriesGetPixels(Point center, Size size)
        {
            return GatherMapPixels(center, size, GatherMapMode.Territories);
        }

        public static void TerritoriesSetPixel(Point p, System.Drawing.Color c)
        {
            if (!IsPointSafe(p)) return;
            territories.SetPixel(p.X, p.Y, c);
            territories_dirtyBits = true;
            Console.WriteLine(territories.GetPixel(p.X, p.Y));
        }

        public static bool InitializeNewPlayer(EkumenaPlayerStruct player, Point startingPoint, System.Drawing.Color c)
        {
            if (!EkumenaStorage.RegisteredPlayersSafeCheck(c)) return false;

            EkumenaStorage.RegisterPlayer(player.User, c);
            EkumenaPlayer Player = new EkumenaPlayer(player);
            EkumenaStorage.AddPlayer(Player);
            var points = CircleDetection(startingPoint, 10);

            foreach (Point p in points)
                TerritoriesSetPixel(p, Player.Color);

            BuildingsSetPixel(startingPoint, townColor);

            return true;
        }

        public static bool PointIsInRange(Point center, Point toCheck, int distance)
        {
            var info = CircleDetection(center, distance);
            foreach (Point p in info)
                if (p == toCheck)
                    return true;
            return false;
        }

        public static Point[] CircleDetection(Point center, int distance)
        {
            List<Point> points = new List<Point>();

            int startXPos = center.X - distance + 1 < 0 ? 0 : center.X - distance + 1;
            int endXPos = center.X + distance > 599 ? 599 : center.X + distance;
            int startYPos = center.Y - distance + 1 < 0 ? 0 : center.Y - distance + 1;
            int endYPos = center.Y + distance > 599 ? 599 : center.Y + distance;

            for (int x = startXPos; x < endXPos; x++)
            {
                for (int y = startYPos; y < endYPos; y++)
                {
                    if (Math.Abs(x - center.X) + Math.Abs(y - center.Y) > distance) continue;
                    points.Add(new Point(x, y));
                }
            }
            return points.ToArray();
        }

        public static Point[] GetPlayerTerritories(ulong u)
        {
            List<Point> Points = new List<Point>();
            for (int x = 0; x < territories.Width; x++) {
                for (int y = 0; y < territories.Height; y++)
                {
                    var pix = territories.GetPixel(x, y);
                    if (pix.R != 0)
                    {
                        var col = EkumenaStorage.RegisteredPlayers[u];
                        if (col == pix)
                            Points.Add(new Point(x,y));
                    }
                }
            }
            return Points.ToArray();
        }

        public static MapInfoStruct[] GetNormalMapPositions(Point[] points)
        {
            List<MapInfoStruct> info = new List<MapInfoStruct>();

            foreach (Point p in points) {
                if (!IsPointSafe(p))
                {
                    info.Add(new MapInfoStruct());
                    continue;
                }

                info.Add(new MapInfoStruct(p
                         , (ColorInfo)EkumenaStorage.ColorSum(mainMap.GetPixel(p.X, p.Y))
                         , (ColorInfo)EkumenaStorage.ColorSum(resources.GetPixel(p.X, p.Y))
                         , (ColorInfo)EkumenaStorage.ColorSum(rareResources.GetPixel(p.X, p.Y))
                         , (BuildingInfo)EkumenaStorage.ColorSum(buildings.GetPixel(p.X, p.Y))));
            }

            return info.ToArray();
        }

        public static MapInfoStruct GetNormalMapPositions(Point point)
        {
            if (!IsPointSafe(point)) return new MapInfoStruct();
            return new MapInfoStruct(point
                         , (ColorInfo)EkumenaStorage.ColorSum(mainMap.GetPixel(point.X, point.Y))
                         , (ColorInfo)EkumenaStorage.ColorSum(resources.GetPixel(point.X, point.Y))
                         , (ColorInfo)EkumenaStorage.ColorSum(rareResources.GetPixel(point.X, point.Y))
                         , (BuildingInfo)EkumenaStorage.ColorSum(buildings.GetPixel(point.X, point.Y)));
        }

        public static void InitMap()
        {
            if (!File.Exists(EKUMPath + "MapLayers/Main.png")) throw new Exception("BRAK MAPY DEBILU.");
            mainMap = new Bitmap(ImageTools.LoadImage(EKUMPath + "MapLayers/Main.png").Image);

            if (!File.Exists(EKUMPath + "MapLayers/Resources.png")) throw new Exception("BRAK MAPY DEBILU.");
            resources = new Bitmap(ImageTools.LoadImage(EKUMPath + "MapLayers/Resources.png").Image);

            if (!File.Exists(EKUMPath + "MapLayers/SpecialResources.png")) throw new Exception("BRAK MAPY DEBILU.");
            rareResources = new Bitmap(ImageTools.LoadImage(EKUMPath + "MapLayers/SpecialResources.png").Image);

            if (!File.Exists(EKUMPath + "MapLayers/Buildings.png")) throw new Exception("BRAK MAPY DEBILU.");
            buildings = new Bitmap(ImageTools.LoadImage(EKUMPath + "MapLayers/Buildings.png").Image);

            if (!File.Exists(EKUMPath + "MapLayers/Territories.png")) throw new Exception("BRAK MAPY DEBILU.");
            territories = new Bitmap(ImageTools.LoadImage(EKUMPath + "MapLayers/Territories.png").Image);

            if (!File.Exists(EKUMPath + "MapLayers/StartPositions.png")) throw new Exception("BRAK MAPY DEBILU.");
            Bitmap map = new Bitmap(ImageTools.LoadImage(EKUMPath + "MapLayers/StartPositions.png").Image);

            List<Point> Points = new List<Point>();

            for (int x = 1; x < map.Width - 1; x++) {
                for (int y = 1; y < map.Height - 1; y++)
                {
                    var pix = map.GetPixel(x, y);
                    if (pix.R != 0)
                        Points.Add(new Point(x, y));
                }
            }

            SpawnPoints = Points.ToArray();
            EkumenaStorage.InitStorage();
            EkumenaInterpreter.InitInterpreter();
        }
    }

    public struct RegisteredPlayersStruct
    {
        public ulong user;
        public System.Drawing.Color color;

        public RegisteredPlayersStruct(ulong user, System.Drawing.Color color)
        {
            this.user = user;
            this.color = color;
        }
    }
    
    public struct MapInfoStruct
    {
        public Point point;
        public ColorInfo info;
        public ColorInfo resType;
        public ColorInfo rareType;
        public BuildingInfo buildingType;

        public MapInfoStruct(Point point, ColorInfo info, ColorInfo resType, ColorInfo rareType, BuildingInfo buildingType)
        {
            this.point = point;
            this.info = info;
            this.resType = resType;
            this.rareType = rareType;
            this.buildingType = buildingType;
        }
    }

    public struct EkumenaResource
    {
        public ResourceType resourceName;
        public int amount;

        public static EkumenaResource operator +(EkumenaResource str1, EkumenaResource str2)
        {
            str1.amount += str2.amount;
            return str1;
        }

        public static EkumenaResource operator -(EkumenaResource str1, EkumenaResource str2)
        {
            str1.amount -= str2.amount;
            return str1;
        }
    }

    public enum GatherMapMode
    {
        MainMap,
        Resources,
        RareResources,
        Territories,
        Buildings
    }

    public enum ResearchType
    { 
        Known,
        InProgress,
        Option
    }

    public enum IntelType
    {
        Visible,
        Resource,
        Others
    }

    public enum ResourceType
    {
        None,
        Money,
        Pop,
        Food,
        Happiness,
        BuildingResource,
        Copper,
        Metal,
        Oil
    }

    public enum ActionType
    {
        AddResource,
        RemoveResource
    }

    public enum BuildingInfo : ushort
    { 
        None = 0,
        Town = 555,
        City = 455,
        Tower = 140,
        Farm = 370
    }

    public enum ColorInfo : ushort
    {
        NotUsable = 0,
        StartPos = 486,
        DeepWater = 250,
        Water = 440,
        Lake = 322,
        Snow = 765,
        Sand = 493,
        Forest = 138,
        Mountain = 429,
        Copper = 346,
        Iron = 150,
        Oil = 54,
        Bridge = 190
    }
}