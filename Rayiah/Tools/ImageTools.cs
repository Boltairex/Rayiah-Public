using ImageProcessor;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace Rayiah.Tools
{
    static class ImageTools
    {
        public static ImageFactory LoadImage(string path)
        {
            byte[] photoBytes = File.ReadAllBytes(path);
            using (MemoryStream inStream = new MemoryStream(photoBytes))
            {
                ImageFactory imageFactory = new ImageFactory(preserveExifData: true);
                imageFactory.Load(path);
                return imageFactory;
            }
        }

        public static async Task<Image> DownloadImage(string url)
        {
            using (HttpClient http = new HttpClient())
            {
                var web = await http.GetAsync(url);
                return (BytesToImage(web.Content.ReadAsByteArrayAsync().Result));
            }
        }

        public static async Task<Stream> DownloadStreamImage(string url)
        {
            var values = url.Split('.');
            var end = values[values.Length - 1].ToLower();

            ImageFormat format;
            switch(end)
            {
                case "png":
                    format = ImageFormat.Png;
                    break;
                case "jpeg":
                    format = ImageFormat.Jpeg;
                    break;
                case "gif":
                    format = ImageFormat.Gif;
                    break;
                case "bmp":
                    format = ImageFormat.Bmp;
                    break;
                default:
                    format = ImageFormat.MemoryBmp;
                    break;
            }

            using (HttpClient http = new HttpClient())
            {
                var web = await http.GetAsync(url);
                var image = Image.FromStream(web.Content.ReadAsStreamAsync().Result);
                var imgStream = new MemoryStream();

                image.Save(imgStream, format);
                return imgStream;
            }
        }

        public static async Task<byte[]> DownloadRawImage(string url)
        {
            using (HttpClient http = new HttpClient())
            {
                var web = await http.GetAsync(url);
                return web.Content.ReadAsByteArrayAsync().Result;
            }
        }

        public static Image BytesToImage(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return Image.FromStream(stream);
            }
        }

        public static Bitmap ColorsToBitmap(Point[] points, Color[] colors, int xSize, int ySize)
        {
            if (points is null) throw new Exception("Null value passed");
            if (points.Length == 0) throw new Exception("Null value passed");

            int lowestX = points[0].X;
            int lowestY = points[0].Y;
            foreach (Point p in points)
            {
                if (p.X < lowestX)
                    lowestX = p.X;
                if (p.Y < lowestY)
                    lowestY = p.Y;
            }
            Console.WriteLine(lowestX);
            Console.WriteLine(lowestY);

            for (int x = 0; x < points.Length; x++)
            {
                points[x].X -= lowestX;
                points[x].Y -= lowestY;
            }

            Bitmap map = new Bitmap(xSize, ySize);
            for (int x = 0; x < points.Length; x++)
                map.SetPixel(points[x].X, points[x].Y, colors[x]);

            return map;
        }

        /// <summary>
        /// Lossy resize (linear interpolation)
        /// </summary>
        /// <param name="imgToResize"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Image ResizeImage(Image imgToResize, Size size)
        {
            return new Bitmap(imgToResize, size);
        }

        /// <summary>
        /// Standard resize (without interpolation)
        /// </summary>
        /// <param name="imgToUpscale"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Image UpscaleImage(Image imgToUpscale, int count)
        {
            if (count < 1) count = 1;
            Bitmap toScale = new Bitmap(imgToUpscale);
            Bitmap map = new Bitmap(imgToUpscale.Width * count, imgToUpscale.Height * count);
            for (int x = 0; x < imgToUpscale.Width * count; x+=count) {
                for (int y = 0; y < imgToUpscale.Height * count; y+=count) {
                    for (int xScale = 0; xScale < count; xScale++) {
                        for (int yScale = 0; yScale < count; yScale++)
                        {
                            int xsc = x + xScale;
                            int ysc = y + yScale;
                            map.SetPixel(xsc, ysc, toScale.GetPixel((int)Math.Floor((float)xsc / count), (int)Math.Floor((float)ysc / count)));
                        }
                    }
                }
            }
            return map;
        }

        [Obsolete("Didn't work.")]
        public static Image DrawText(string text, Font font, Color textColor)
        {
            using (Graphics drawing = Graphics.FromImage(new Bitmap(1, 1)))
            {
                SizeF textSize = drawing.MeasureString(text, font);
                using (Image img = new Bitmap((int)textSize.Width, (int)textSize.Height))
                {
                    Brush textBrush = new SolidBrush(textColor);

                    drawing.DrawString(text, font, textBrush, 0, 0);

                    drawing.Save();

                    textBrush.Dispose();
                    drawing.Dispose();

                    return img;
                }
            }
        }
    }
}