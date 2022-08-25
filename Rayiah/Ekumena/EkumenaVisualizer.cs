using System;
using System.Drawing;
using ImageProcessor;
using ImageProcessor.Imaging;
using Rayiah.Tools;
using System.IO;

namespace Rayiah.Ekumena
{
    public static class EkumenaVisualizer
    {
        public static Image GenerateTerrain(EkumenaPlayer player, Point center, GenerateOptions options, int Cells = 1)
        {
            Cells = Cells < 1 ? 1 : Cells > 12 ? 12 : Cells;

            try
            {
                Size size = new Size(50 * Cells, 50 * Cells);
                center = EkumenaLoader.ProbePoint(center, size);
                Console.WriteLine(center);
                
                var xTranslatedPos = center.X - size.Width / 2 + 1;
                xTranslatedPos = xTranslatedPos < 0 ? 0 : xTranslatedPos > 599 ? 599 : xTranslatedPos;
                var yTranslatedPos = center.Y - size.Height / 2 + 1;
                yTranslatedPos = yTranslatedPos < 0 ? 0 : yTranslatedPos > 599 ? 599 : yTranslatedPos;

                ImageFactory MainMap = new ImageFactory();
                MainMap.Quality(100);
                var (mainPoints, mainColors) = EkumenaLoader.MainMapGetPixels(center, size);
                Bitmap MainBitmap = ImageTools.ColorsToBitmap(mainPoints, mainColors, size.Width, size.Height);

                foreach (Point mp in mainPoints)
                {
                    //var translatedPoint = new Point(mp.X + xTranslatedPos, mp.Y + yTranslatedPos);
                    //if (!player.intel.ContainsKey(translatedPoint))
                    //    MainBitmap.SetPixel(mp.X, mp.Y, Color.Black);
                }

                MainMap.Load(MainBitmap);

                if (options.withResources)
                {
                    ImageLayer resources = new ImageLayer();
                    resources.Opacity = 100;
                    var (points, colors) = EkumenaLoader.ResourceGetPixels(center, size);
                    var b = ImageTools.ColorsToBitmap(points, colors, size.Width, size.Height);

                    foreach (Point p in points)
                    {
                        var translatedPoint = new Point(p.X + xTranslatedPos, p.Y + yTranslatedPos);
                        if (!player.Intel.ContainsKey(translatedPoint))
                            b.SetPixel(p.X, p.Y, Color.Transparent);
                    }

                    resources.Image = b;
                    MainMap.Overlay(resources);
                }

                if (options.withRareResources)
                {
                    ImageLayer rareResources = new ImageLayer();
                    rareResources.Opacity = 100;
                    var (points, colors) = EkumenaLoader.ResourceGetPixels(center, size);
                    var b = ImageTools.ColorsToBitmap(points, colors, size.Width, size.Height);

                    foreach (Point p in points)
                    {
                        var translatedPoint = new Point(p.X + xTranslatedPos, p.Y + yTranslatedPos);
                        if (!player.Intel.ContainsKey(translatedPoint) || !player.Intel[translatedPoint].r)
                            b.SetPixel(p.X, p.Y, Color.Transparent);
                    }

                    rareResources.Image = b;
                    MainMap.Overlay(rareResources);
                }

                if (options.withTerritories)
                {
                    ImageLayer territories = new ImageLayer();
                    territories.Opacity = 60;
                    var (points, colors) = EkumenaLoader.TerritoriesGetPixels(center, size);
                    var b = ImageTools.ColorsToBitmap(points, colors, size.Width, size.Height);

                    foreach (Point p in points)
                    {
                        var translatedPoint = new Point(p.X + xTranslatedPos, p.Y + yTranslatedPos);
                        if (!player.Intel.ContainsKey(translatedPoint))
                            b.SetPixel(p.X, p.Y, Color.Transparent);
                    }

                    territories.Image = b;
                    MainMap.Overlay(territories);
                }
                if (options.withBuildings)
                {
                    ImageLayer buildings = new ImageLayer();
                    buildings.Opacity = 100;
                    var (points, colors) = EkumenaLoader.BuildingsGetPixels(center, size);
                    var b = ImageTools.ColorsToBitmap(points, colors, size.Width, size.Height);

                    foreach (Point p in points)
                    {
                        var translatedPoint = new Point(p.X + xTranslatedPos, p.Y + yTranslatedPos);
                        if (!player.Intel.ContainsKey(translatedPoint) || !player.Intel[translatedPoint].o)
                            b.SetPixel(p.X, p.Y, Color.Transparent);
                    }

                    buildings.Image = b;
                    MainMap.Overlay(buildings);
                }

                using (var mem = new MemoryStream())
                {
                    MainMap.Save(mem);
                    return ImageTools.UpscaleImage(Image.FromStream(mem), 6 / Cells);
                }
            }catch(Exception e) { Console.WriteLine(e); }
            return null;
        }
    }

    public struct GenerateOptions
    {
        public bool withTerritories;
        public bool withResources;
        public bool withRareResources;
        public bool withBuildings;

        public GenerateOptions(bool wt, bool wr, bool wrr, bool wb)
        {
            withTerritories = wt;
            withResources = wr;
            withRareResources = wrr;
            withBuildings = wb;
        }
    }
}
