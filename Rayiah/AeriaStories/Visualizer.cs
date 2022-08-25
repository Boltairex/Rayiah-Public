using Rayiah.Management;
using Rayiah.AeriaStories.Objects;
using ImageProcessor;
using System.Drawing;
using Rayiah.Tools;

namespace Rayiah.AeriaStories
{
    public static class Visualizer
    {
        public static void GenerateCharacter(Equipment eq)
        {
            if (eq.info.imgPath == null) return;

            ImageFactory img = new ImageFactory(); // ImageTools.DownloadImage(eq.info.imgPath).Result;
            img.Load(ImageTools.DownloadRawImage(eq.info.imgPath).Result);
        }
    }
}
