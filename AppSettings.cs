using System;
using System.IO;
using System.Text.Json;

namespace radiant_noether
{
    public class AppSettings
    {
        public string Member { get; set; } = "woni";
        public string Language { get; set; } = "ko";
        public string Theme { get; set; } = "dark";
        public bool IsLightTheme => Theme.Equals("light", StringComparison.OrdinalIgnoreCase);

        // 타이머 위젯 스타일: "compact" (간소화), "standard" (표준), "ring" (원형 링 & 통계)
        public string TimerStyle { get; set; } = "compact";

        public int FocusMinutes { get; set; } = 25;
        public int BreakMinutes { get; set; } = 5;
        public int LongBreakMinutes { get; set; } = 15;
        public int LongBreakInterval { get; set; } = 4;
        public bool AlwaysOnTop { get; set; } = true;
        public double CharacterSize { get; set; } = 220;
        public double CharacterOpacity { get; set; } = 1.0;
        public bool AlwaysShowTime { get; set; } = false;
        public bool PlaySound { get; set; } = true;
        public double? WindowLeft { get; set; }
        public double? WindowTop { get; set; }

        // 오늘 / 누적 통계
        public string StatsDate { get; set; } = "";
        public int TodayCompletedSessions { get; set; } = 0;
        public int TotalCompletedSessions { get; set; } = 0;
        public int TodayFocusMinutes { get; set; } = 0;

        public void CheckAndResetDailyStats()
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            if (StatsDate != today)
            {
                StatsDate = today;
                TodayCompletedSessions = 0;
                TodayFocusMinutes = 0;
                Save();
            }
        }

        private static readonly string SettingsFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        settings.CheckAndResetDailyStats();
                        return settings;
                    }
                }
            }
            catch
            {
                // 기본값 사용
            }
            var newSettings = new AppSettings();
            newSettings.CheckAndResetDailyStats();
            newSettings.Save();
            return newSettings;
        }

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
                // 예외 무시
            }
        }
    }
}
