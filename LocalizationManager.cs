using System;
using System.Collections.Generic;

namespace radiant_noether
{
    public static class LocalizationManager
    {
        public static string CurrentLanguage { get; set; } = "ko";

        public static bool IsEnglish => CurrentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase);

        public static string Get(string key)
        {
            if (IsEnglish && EnStrings.TryGetValue(key, out var enVal))
                return enVal;

            if (KoStrings.TryGetValue(key, out var koVal))
                return koVal;

            return key;
        }

        private static readonly Dictionary<string, string> KoStrings = new()
        {
            // 타이머 위젯
            { "Focus", "집중" },
            { "Break", "휴식" },
            { "LongBreak", "긴 휴식" },
            { "Start", "▶ 시작" },
            { "Pause", "⏸ 일시정지" },
            { "Skip", "⏭ 건너뛰기" },
            { "Reset", "↺" },
            { "Settings", "⚙" },
            { "ResetTooltip", "타이머 초기화" },
            { "SettingsTooltip", "설정" },

            // 우클릭 컨텍스트 메뉴
            { "MenuStart", "▶ 타이머 시작" },
            { "MenuPause", "⏸ 일시정지" },
            { "MenuSkip", "⏭ 다음 단계로 건너뛰기" },
            { "MenuReset", "↺ 타이머 초기화" },
            { "MenuSelectMember", "👤 리센느 멤버 선택" },
            { "MenuAlwaysOnTop", "📌 항상 맨 위에 표시" },
            { "MenuAlwaysShowTime", "💬 타이머 시간 항상 표시" },
            { "MenuOpenImages", "📂 캐릭터 이미지 폴더 열기..." },
            { "MenuOpenSounds", "🎵 멤버별 음성 폴더 열기..." },
            { "MenuToggleTheme", "🌓 테마 변경 (화이트/블랙)" },
            { "MenuSettings", "⚙ 상세 설정..." },
            { "MenuExit", "❌ 프로그램 종료" },

            // 설정창 헤더 & 섹션
            { "SettingsTitle", "설정 - RESCENE 뽀모도로" },
            { "SettingsHeaderTitle", "RESCENE 타이머 설정" },
            { "SettingsHeaderSubtitle", "바탕화면 뽀모도로 위젯의 멤버, 시간 및 표시 옵션을 설정합니다." },
            { "SectionTheme", "테마 디자인" },
            { "ThemeDesc", "위젯과 설정창의 컬러 테마(화이트/블랙)를 선택합니다." },
            { "ThemeDark", "🌙 다크 모드 (블랙)" },
            { "ThemeLight", "☀️ 라이트 모드 (화이트)" },
            { "SectionLanguage", "언어 / Language" },
            { "LangKo", "Korean(한국어)" },
            { "LangEn", "English(영어)" },
            { "SectionMember", "대표 멤버" },
            { "MemberDesc", "위젯에 표시할 리센느 멤버를 선택하세요." },
            { "SectionTimer", "시간 및 타이머" },
            { "CardFocusTitle", "집중 시간" },
            { "CardFocusDesc", "학습 또는 업무에 몰입하는 시간" },
            { "CardBreakTitle", "짧은 휴식" },
            { "CardBreakDesc", "집중 사이클 직후 주어지는 휴식 시간" },
            { "CardLongBreakTitle", "긴 휴식" },
            { "CardLongBreakDesc", "4회 집중 완료 후 주어지는 긴 휴식 시간" },
            { "SectionAppearance", "위젯 디자인 및 표시" },
            { "CardSizeTitle", "캐릭터 크기" },
            { "CardSizeDesc", "화면에 표시되는 위젯의 너비/높이" },
            { "CardOpacityTitle", "불투명도" },
            { "CardOpacityDesc", "위젯의 투명도 수준" },
            { "CardAlwaysOnTopTitle", "항상 맨 위에 표시" },
            { "CardAlwaysOnTopDesc", "다른 모든 창보다 위에 상주합니다 (Always on Top)" },
            { "CardAlwaysShowTimeTitle", "타이머 시간 항상 표시" },
            { "CardAlwaysShowTimeDesc", "끄면 마우스를 올렸을 때만 도크가 표시됩니다" },
            { "CardSoundTitle", "멤버 음성 / 알림음 재생" },
            { "CardSoundDesc", "집중 ↔ 휴식 전환 시 음성 또는 알림 소리 재생" },
            { "BtnImages", "📁 이미지 폴더" },
            { "BtnImagesTooltip", "멤버별 캐릭터 이미지(.png)를 관리합니다." },
            { "BtnSounds", "🎵 음성 폴더" },
            { "BtnSoundsTooltip", "멤버별 음성 파일(.mp3, .wav)을 관리합니다." },
            { "BtnSave", "저장 및 적용" },
            { "UnitMinutes", "분" },

            // 타이머 스타일 관련
            { "SectionTimerStyle", "타이머 위젯 스타일" },
            { "TimerStyleDesc", "위젯의 크기와 타이머 정보 표시 방식을 선택합니다." },
            { "TimerStyleCompact", "간소화" },
            { "TimerStyleCompactDesc", "가볍게 볼 수 있는 심플 도크 (세션 번호 포함)" },
            { "TimerStyleStandard", "표준" },
            { "TimerStyleStandardDesc", "모드 탭, 대형 타이머, 세션 진행 바" },
            { "TimerStyleRing", "원형 링 & 통계" },
            { "TimerStyleRingDesc", "원형 시계 링, 세션 도트, 오늘/누적 통계 카드" },
            { "MenuTimerStyle", "⏱ 타이머 스타일" },
            { "ModePomodoro", "집중" },
            { "ModeShortBreak", "짧은 휴식" },
            { "ModeLongBreak", "긴 휴식" },
            { "StatToday", "오늘 완료" },
            { "StatTotal", "누적 완료" },
            { "StatTime", "집중 시간" },
            { "SessionLabel", "세션" },
            { "DailyProgress", "일일 달성도" },
        };

        private static readonly Dictionary<string, string> EnStrings = new()
        {
            // 타이머 위젯
            { "Focus", "Focus" },
            { "Break", "Break" },
            { "LongBreak", "Long Break" },
            { "Start", "▶ Start" },
            { "Pause", "⏸ Pause" },
            { "Skip", "⏭ Skip" },
            { "Reset", "↺" },
            { "Settings", "⚙" },
            { "ResetTooltip", "Reset Timer" },
            { "SettingsTooltip", "Settings" },

            // 우클릭 컨텍스트 메뉴
            { "MenuStart", "▶ Start Timer" },
            { "MenuPause", "⏸ Pause Timer" },
            { "MenuSkip", "⏭ Skip to Next" },
            { "MenuReset", "↺ Reset Timer" },
            { "MenuSelectMember", "👤 Select RESCENE Member" },
            { "MenuAlwaysOnTop", "📌 Always on Top" },
            { "MenuAlwaysShowTime", "💬 Always Show Timer Dock" },
            { "MenuOpenImages", "📂 Open Character Images Folder..." },
            { "MenuOpenSounds", "🎵 Open Voice Audio Folder..." },
            { "MenuToggleTheme", "🌓 Switch Theme (Light/Dark)" },
            { "MenuSettings", "⚙ Settings..." },
            { "MenuExit", "❌ Exit Program" },

            // 설정창 헤더 & 섹션
            { "SettingsTitle", "Settings - RESCENE Pomodoro" },
            { "SettingsHeaderTitle", "RESCENE Timer Settings" },
            { "SettingsHeaderSubtitle", "Configure mascot member, pomodoro intervals, and display options." },
            { "SectionTheme", "Color Theme" },
            { "ThemeDesc", "Choose color aesthetic (Light Mode / Dark Mode) for widget and settings." },
            { "ThemeDark", "🌙 Dark Mode" },
            { "ThemeLight", "☀️ Light Mode" },
            { "SectionLanguage", "Language / 언어" },
            { "LangKo", "Korean(한국어)" },
            { "LangEn", "English(영어)" },
            { "SectionMember", "Active Member" },
            { "MemberDesc", "Select a RESCENE member to display on your desktop widget." },
            { "SectionTimer", "Intervals & Timer" },
            { "CardFocusTitle", "Focus Duration" },
            { "CardFocusDesc", "Time dedicated to study or deep work" },
            { "CardBreakTitle", "Short Break" },
            { "CardBreakDesc", "Short recharge interval after each focus session" },
            { "CardLongBreakTitle", "Long Break" },
            { "CardLongBreakDesc", "Extended break after completing 4 focus sessions" },
            { "SectionAppearance", "Widget Appearance & Display" },
            { "CardSizeTitle", "Character Size" },
            { "CardSizeDesc", "Width and height of the desktop mascot widget" },
            { "CardOpacityTitle", "Opacity" },
            { "CardOpacityDesc", "Transparency level of the widget" },
            { "CardAlwaysOnTopTitle", "Always on Top" },
            { "CardAlwaysShowTimeTitle", "Always Show Timer Dock" },
            { "CardAlwaysShowTimeDesc", "If turned off, dock only appears on hover" },
            { "CardSoundTitle", "Member Voices & Sound Alerts" },
            { "CardSoundDesc", "Play member audio clips when switching modes" },
            { "BtnImages", "📁 Images Folder" },
            { "BtnImagesTooltip", "Manage character PNG images for each member." },
            { "BtnSounds", "🎵 Audio Folder" },
            { "BtnSoundsTooltip", "Manage member voice audio clips (.mp3, .wav)." },
            { "BtnSave", "Save & Apply" },
            { "UnitMinutes", "min" },

            // Timer Style Related
            { "SectionTimerStyle", "Timer Widget Style" },
            { "TimerStyleDesc", "Choose widget size and information density layout." },
            { "TimerStyleCompact", "Compact" },
            { "TimerStyleCompactDesc", "Minimal dock with session number counter" },
            { "TimerStyleStandard", "Standard" },
            { "TimerStyleStandardDesc", "Mode tabs, large timer, session progress bar" },
            { "TimerStyleRing", "Ring & Stats" },
            { "TimerStyleRingDesc", "Clock progress ring, session dots, stats cards" },
            { "MenuTimerStyle", "⏱ Timer Style" },
            { "ModePomodoro", "Pomodoro" },
            { "ModeShortBreak", "Short Break" },
            { "ModeLongBreak", "Long Break" },
            { "StatToday", "Today" },
            { "StatTotal", "Total" },
            { "StatTime", "Time" },
            { "SessionLabel", "Session" },
            { "DailyProgress", "Daily Progress" },
        };
    }
}
