using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace radiant_noether
{
    public static class ImageProcessor
    {
        public static void ConvertJpgToTransparentPng(string inputJpg, string outputPng)
        {
            var uri = new Uri(inputJpg, UriKind.Absolute);
            var srcBitmap = new BitmapImage();
            srcBitmap.BeginInit();
            srcBitmap.UriSource = uri;
            srcBitmap.CacheOption = BitmapCacheOption.OnLoad;
            srcBitmap.EndInit();

            int width = srcBitmap.PixelWidth;
            int height = srcBitmap.PixelHeight;

            var formatConverted = new FormatConvertedBitmap(srcBitmap, PixelFormats.Bgra32, null, 0);
            int stride = width * 4;
            byte[] pixels = new byte[stride * height];
            formatConverted.CopyPixels(pixels, stride, 0);

            // BFS Flood-fill to find connected outer black background
            bool[,] isOuterBg = new bool[width, height];
            var queue = new Queue<PointInt>();

            bool IsNearBlack(int x, int y)
            {
                int idx = y * stride + x * 4;
                byte b = pixels[idx];
                byte g = pixels[idx + 1];
                byte r = pixels[idx + 2];
                // Tolerance threshold for jpeg black
                return r <= 28 && g <= 28 && b <= 28;
            }

            void TryEnqueue(int x, int y)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return;
                if (isOuterBg[x, y]) return;

                if (IsNearBlack(x, y))
                {
                    isOuterBg[x, y] = true;
                    queue.Enqueue(new PointInt(x, y));
                }
            }

            // Enqueue all boundary pixels
            for (int x = 0; x < width; x++)
            {
                TryEnqueue(x, 0);
                TryEnqueue(x, height - 1);
            }
            for (int y = 0; y < height; y++)
            {
                TryEnqueue(0, y);
                TryEnqueue(width - 1, y);
            }

            // Run BFS
            while (queue.Count > 0)
            {
                var pt = queue.Dequeue();
                int px = pt.X;
                int py = pt.Y;

                TryEnqueue(px + 1, py);
                TryEnqueue(px - 1, py);
                TryEnqueue(px, py + 1);
                TryEnqueue(px, py - 1);
            }

            // Set alpha to 0 for all outer background pixels
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * stride + x * 4;
                    if (isOuterBg[x, y])
                    {
                        pixels[idx] = 0;
                        pixels[idx + 1] = 0;
                        pixels[idx + 2] = 0;
                        pixels[idx + 3] = 0; // Transparent
                    }
                }
            }

            // Edge smoothing / defringe
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if (isOuterBg[x, y]) continue;

                    // If neighbor is outer background, check if pixel is dark border
                    bool hasBgNeighbor = isOuterBg[x - 1, y] || isOuterBg[x + 1, y] || isOuterBg[x, y - 1] || isOuterBg[x, y + 1];
                    if (hasBgNeighbor)
                    {
                        int idx = y * stride + x * 4;
                        byte b = pixels[idx];
                        byte g = pixels[idx + 1];
                        byte r = pixels[idx + 2];
                        int maxVal = Math.Max(r, Math.Max(g, b));

                        if (maxVal < 60)
                        {
                            // Soften transition
                            pixels[idx + 3] = (byte)Math.Clamp((maxVal * 255) / 60, 0, 255);
                        }
                    }
                }
            }

            // Create WriteableBitmap and save as PNG
            var outputBitmap = BitmapSource.Create(
                width, height, 96, 96,
                PixelFormats.Bgra32, null,
                pixels, stride);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(outputBitmap));

            string? dir = Path.GetDirectoryName(outputPng);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var fs = File.Open(outputPng, FileMode.Create, FileAccess.Write);
            encoder.Save(fs);
        }

        private readonly struct PointInt
        {
            public readonly int X;
            public readonly int Y;
            public PointInt(int x, int y) { X = x; Y = y; }
        }
    }
}
