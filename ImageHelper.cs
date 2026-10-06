using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace radiant_noether
{
    public static class ImageHelper
    {
        public static string ImagesDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

        public static void EnsureImagesExist(bool forceRecreate = false)
        {
            if (!Directory.Exists(ImagesDirectory))
            {
                Directory.CreateDirectory(ImagesDirectory);
            }

            // 사용자 업로드 이미지 자동 변환 처리
            ProcessUploadedImages();

            foreach (var member in MemberInfo.Members)
            {
                string focusPath = Path.Combine(ImagesDirectory, $"{member.Id}_focus.png");
                string breakPath = Path.Combine(ImagesDirectory, $"{member.Id}_break.png");

                if (forceRecreate || !File.Exists(focusPath))
                {
                    if (!TryExtractEmbeddedImage(member.Id, isFocus: true, focusPath))
                    {
                        GenerateDummyImage(member, isFocus: true, focusPath);
                    }
                }
                if (forceRecreate || !File.Exists(breakPath))
                {
                    if (!TryExtractEmbeddedImage(member.Id, isFocus: false, breakPath))
                    {
                        GenerateDummyImage(member, isFocus: false, breakPath);
                    }
                }
            }
        }

        private static bool TryExtractEmbeddedImage(string memberId, bool isFocus, string destPath)
        {
            try
            {
                var assembly = typeof(ImageHelper).Assembly;
                string stateKey = isFocus ? "focus" : "break";
                string resName = $"radiant_noether.Images.{memberId}_{stateKey}.png";
                using var stream = assembly.GetManifestResourceStream(resName);
                if (stream != null)
                {
                    using var fs = File.Create(destPath);
                    stream.CopyTo(fs);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to extract embedded image {memberId}: {ex.Message}");
            }
            return false;
        }

        public static void ProcessUploadedImages()
        {
            // 1. 사용자의 로컬 원본 투명 PNG 폴더 우선 확인
            string localPomodoroDir = @"C:\Users\psg00\Documents\Codex\2026-10-02\new-chat\outputs\pomodoro-characters";
            if (Directory.Exists(localPomodoroDir))
            {
                var localMapping = new (string MemberId, bool IsFocus, string FileName)[]
                {
                    ("woni", true, "woni-focus-v5.png"),
                    ("woni", false, "woni-rest-v2.png"),
                    ("minami", true, "minami-focus.png"),
                    ("minami", false, "minami-rest.png"),
                    ("may", true, "may-focus.png"),
                    ("may", false, "may-rest.png"),
                    ("zena", true, "jena-focus.png"),
                    ("zena", false, "jena-rest.png"),
                    ("liv", true, "liv-focus.png"),
                    ("liv", false, "liv-rest.png"),
                };

                foreach (var item in localMapping)
                {
                    string src = Path.Combine(localPomodoroDir, item.FileName);
                    string dst = Path.Combine(ImagesDirectory, $"{item.MemberId}_{(item.IsFocus ? "focus" : "break")}.png");

                    if (File.Exists(src))
                    {
                        try
                        {
                            File.Copy(src, dst, true);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error copying {item.FileName}: {ex.Message}");
                        }
                    }
                }
            }
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, BitmapImage> _imageCache = new();

        public static void ClearCache()
        {
            _imageCache.Clear();
        }

        public static ImageSource? LoadMemberImage(string memberId, bool isFocus)
        {
            try
            {
                EnsureImagesExist();

                string stateKey = isFocus ? "focus" : "break";
                string cacheKey = $"{memberId}_{stateKey}";

                if (_imageCache.TryGetValue(cacheKey, out var cached))
                {
                    return cached;
                }

                string imagePath = Path.Combine(ImagesDirectory, $"{memberId}_{stateKey}.png");

                if (!File.Exists(imagePath))
                {
                    imagePath = Path.Combine(ImagesDirectory, $"woni_{stateKey}.png");
                }

                if (File.Exists(imagePath))
                {
                    byte[] bytes = File.ReadAllBytes(imagePath);
                    using var ms = new MemoryStream(bytes);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 480; // 240~360px 위젯에서 선명함을 유지하면서 메모리 80% 절감
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    _imageCache[cacheKey] = bitmap;
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load image: {ex.Message}");
            }

            return null;
        }

        public static void OpenImagesFolder()
        {
            try
            {
                EnsureImagesExist();
                Process.Start(new ProcessStartInfo
                {
                    FileName = ImagesDirectory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지 폴더를 여는 중 오류가 발생했습니다: {ex.Message}");
            }
        }

        public static void RegenerateHighQualityDummies()
        {
            ClearCache();
            EnsureImagesExist(forceRecreate: true);
        }

        private static void GenerateDummyImage(MemberInfo member, bool isFocus, string outputPath)
        {
            const int width = 480;
            const int height = 520;
            const double dpi = 96;

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                var themeBrush = new SolidColorBrush(member.ThemeColor);
                themeBrush.Freeze();

                var plateBg = isFocus 
                    ? new SolidColorBrush(Color.FromArgb(245, 24, 27, 36))
                    : new SolidColorBrush(Color.FromArgb(245, 26, 38, 32));
                plateBg.Freeze();

                var shadowBrush = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0));
                shadowBrush.Freeze();
                dc.DrawEllipse(shadowBrush, null, new Point(240, 230), 190, 190);

                var outerPen = new Pen(themeBrush, 10);
                outerPen.Freeze();
                dc.DrawEllipse(plateBg, outerPen, new Point(240, 220), 180, 180);

                var innerPen = new Pen(new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)), 3);
                innerPen.Freeze();
                dc.DrawEllipse(null, innerPen, new Point(240, 220), 164, 164);

                void DrawCenteredText(string text, double y, double size, Brush brush, FontWeight weight)
                {
                    var ft = new FormattedText(
                        text,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(new FontFamily("Segoe UI, Malgun Gothic"), FontStyles.Normal, weight, FontStretches.Normal),
                        size,
                        brush,
                        dpi);
                    ft.TextAlignment = TextAlignment.Center;
                    dc.DrawText(ft, new Point(240, y));
                }

                DrawCenteredText("RESCENE", 84, 22, new SolidColorBrush(Color.FromArgb(220, 230, 230, 240)), FontWeights.SemiBold);
                DrawCenteredText(member.NameKo, 116, 40, themeBrush, FontWeights.Bold);

                if (isFocus)
                {
                    DrawCenteredText("🔥", 180, 64, Brushes.White, FontWeights.Normal);
                    DrawCenteredText("집중 타임", 264, 32, Brushes.White, FontWeights.Bold);
                    DrawCenteredText("FOCUS MODE", 308, 22, new SolidColorBrush(Color.FromArgb(220, 255, 190, 205)), FontWeights.SemiBold);
                }
                else
                {
                    DrawCenteredText("☕", 180, 64, Brushes.White, FontWeights.Normal);
                    DrawCenteredText("쉬는 시간", 264, 32, new SolidColorBrush(Color.FromArgb(255, 170, 245, 200)), FontWeights.Bold);
                    DrawCenteredText("BREAK TIME", 308, 22, new SolidColorBrush(Color.FromArgb(220, 190, 245, 215)), FontWeights.SemiBold);
                }

                var badgeBrush = new SolidColorBrush(Color.FromArgb(240, 36, 40, 50));
                badgeBrush.Freeze();
                var badgePen = new Pen(themeBrush, 3);
                badgePen.Freeze();
                var badgeRect = new Rect(120, 436, 240, 48);
                dc.DrawRoundedRectangle(badgeBrush, badgePen, badgeRect, 24, 24);

                DrawCenteredText(isFocus ? "POMODORO 25m" : "RECHARGE 5m", 444, 22, themeBrush, FontWeights.Bold);
            }

            var rtb = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var fs = File.Open(outputPath, FileMode.Create, FileAccess.Write);
            encoder.Save(fs);
        }
    }
}
