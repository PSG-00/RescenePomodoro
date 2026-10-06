using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace radiant_noether
{
    public partial class MainWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly PomodoroTimer _timer;
        private SettingsWindow? _settingsWindow;

        public MainWindow()
        {
            InitializeComponent();

            _settings = AppSettings.Load();
            _timer = new PomodoroTimer(_settings);

            _timer.OnTick += Timer_OnTick;
            _timer.OnStateChanged += Timer_OnStateChanged;

            ApplySettings();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 위치 복원 (설정된 위치가 없으면 화면 우하단)
            if (_settings.WindowLeft.HasValue && _settings.WindowTop.HasValue)
            {
                Left = _settings.WindowLeft.Value;
                Top = _settings.WindowTop.Value;
            }
            else
            {
                var workArea = SystemParameters.WorkArea;
                Left = workArea.Right - Width - 60;
                Top = workArea.Bottom - Height - 60;
            }

            // 우클릭 컨텍스트 메뉴 설정
            SetupContextMenu();

            // 필수 폴더 및 기본 에셋(이미지 10종, 음성 안내문) 자동 보장
            ImageHelper.EnsureImagesExist();
            SoundManager.EnsureSoundsFolder();

            // 이미지 로드 및 타이머 UI 초기화
            UpdateCharacterImage();
            UpdateTimerUI();
            UpdateOverlayVisibility(isMouseOver: false);

            // 초기화 완료 후 유휴 상태에서 초기 로드 버퍼/미사용 페이징 즉시 회수 (Working Set 100MB 이하 유지)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MemoryOptimizer.TrimMemory();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

            // 5분 주기로 백그라운드 메모리 자동 최적화 유지
            var memoryTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5)
            };
            memoryTimer.Tick += (s, e) => MemoryOptimizer.TrimMemory();
            memoryTimer.Start();
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _settings.WindowLeft = Left;
            _settings.WindowTop = Top;
            _settings.Save();
        }

        public void ApplySettings()
        {
            LocalizationManager.CurrentLanguage = _settings.Language;
            Topmost = _settings.AlwaysOnTop;
            CharacterImage.Width = _settings.CharacterSize;
            CharacterImage.Height = _settings.CharacterSize;

            // 불투명도 적용 (진행 중인 애니메이션 해제 후 직접 설정)
            CharacterImage.BeginAnimation(OpacityProperty, null);
            CharacterImage.Opacity = _settings.CharacterOpacity;

            _timer.RefreshSettings();
            UpdateCharacterImage(animate: false);
            ApplyTimerStyle(_settings.TimerStyle);
            ApplyTheme();
            UpdateTimerUI();
            UpdateOverlayVisibility(isMouseOver: CharacterContainer.IsMouseOver);
            SetupContextMenu();
        }

        private void ApplyTimerStyle(string style)
        {
            string s = string.IsNullOrEmpty(style) ? "compact" : style.ToLowerInvariant();
            DockCompact.Visibility = s == "compact" ? Visibility.Visible : Visibility.Collapsed;
            DockStandard.Visibility = s == "standard" ? Visibility.Visible : Visibility.Collapsed;
            DockRing.Visibility = s == "ring" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ApplyTheme()
        {
            bool isLight = _settings.IsLightTheme;

            if (isLight)
            {
                // iOS / Samsung One UI 스타일 클린 화이트 모드
                Resources["ToolButtonHoverBrush"] = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));
                Resources["ToolButtonPressedBrush"] = new SolidColorBrush(Color.FromArgb(28, 0, 0, 0));
                Resources["PrimaryButtonHoverBrush"] = new SolidColorBrush(Color.FromArgb(24, 0, 0, 0));
                Resources["PrimaryButtonPressedBrush"] = new SolidColorBrush(Color.FromArgb(36, 0, 0, 0));
                Resources["MenuCheckBrush"] = new SolidColorBrush(Color.FromRgb(0, 122, 255));

                TimerOverlay.Background = new SolidColorBrush(Color.FromArgb(244, 255, 255, 255));
                TimerOverlay.BorderBrush = new SolidColorBrush(Color.FromArgb(32, 0, 0, 0));
                DockShadow.Color = Colors.Black;
                DockShadow.Opacity = 0.12;
                DockShadow.BlurRadius = 18;

                // 1. 간소화 도크 테마
                TimerHeaderCapsule.Background = new SolidColorBrush(Color.FromArgb(12, 0, 0, 0));
                TimerHeaderCapsule.BorderBrush = new SolidColorBrush(Color.FromArgb(18, 0, 0, 0));
                TxtTimer.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                SessionBadgeCompact.Background = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));
                TxtSessionCompact.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));
                MemberBadge.Background = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));
                TxtMember.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));
                DockDivider.Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));

                BtnPlayPause.Background = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));
                BtnPlayPause.BorderBrush = new SolidColorBrush(Color.FromArgb(28, 0, 0, 0));
                BtnPlayPause.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                BtnSkip.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));
                BtnReset.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));
                BtnSettings.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));

                // 2. 표준 도크 테마
                StdModeCapsule.Background = new SolidColorBrush(Color.FromArgb(12, 0, 0, 0));
                StdModeCapsule.BorderBrush = new SolidColorBrush(Color.FromArgb(18, 0, 0, 0));
                TxtTimerStd.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                SessionBadgeStd.Background = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));
                TxtSessionStd.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));
                MemberBadgeStd.Background = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));
                TxtMemberStd.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));
                ProgressStd.Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
                DockDividerStd.Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));

                BtnPlayPauseStd.Background = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));
                BtnPlayPauseStd.BorderBrush = new SolidColorBrush(Color.FromArgb(28, 0, 0, 0));
                BtnPlayPauseStd.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                BtnSkipStd.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));
                BtnResetStd.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));
                BtnSettingsStd.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));

                // 3. 원형 링 도크 테마
                RingModeCapsule.Background = new SolidColorBrush(Color.FromArgb(12, 0, 0, 0));
                RingModeCapsule.BorderBrush = new SolidColorBrush(Color.FromArgb(18, 0, 0, 0));
                RingTrack.Stroke = new SolidColorBrush(Color.FromArgb(24, 0, 0, 0));
                TxtTimerRing.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                TxtRingPercent.Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 128));
                TxtRingSession.Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 108));

                BtnPlayPauseRing.Background = new SolidColorBrush(Color.FromArgb(16, 0, 0, 0));
                BtnPlayPauseRing.BorderBrush = new SolidColorBrush(Color.FromArgb(28, 0, 0, 0));
                BtnPlayPauseRing.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                BtnSkipRing.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));
                BtnResetRing.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));
                BtnSettingsRing.Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 67));

                RingDivider.Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
                RingStatsCard.Background = new SolidColorBrush(Color.FromArgb(12, 0, 0, 0));
                RingStatsCard.BorderBrush = new SolidColorBrush(Color.FromArgb(18, 0, 0, 0));

                TxtStatTodayVal.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                TxtStatTotalVal.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                TxtStatTimeVal.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                TxtStatTodayLbl.Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 138));
                TxtStatTotalLbl.Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 138));
                TxtStatTimeLbl.Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 138));
            }
            else
            {
                // 정제된 모던 다크 모드 (OLED 블랙)
                Resources["ToolButtonHoverBrush"] = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
                Resources["ToolButtonPressedBrush"] = new SolidColorBrush(Color.FromArgb(18, 255, 255, 255));
                Resources["PrimaryButtonHoverBrush"] = new SolidColorBrush(Color.FromArgb(48, 255, 255, 255));
                Resources["PrimaryButtonPressedBrush"] = new SolidColorBrush(Color.FromArgb(26, 255, 255, 255));
                Resources["MenuCheckBrush"] = new SolidColorBrush(Color.FromRgb(224, 64, 96));

                TimerOverlay.Background = new SolidColorBrush(Color.FromArgb(235, 24, 24, 27));
                TimerOverlay.BorderBrush = new SolidColorBrush(Color.FromArgb(24, 255, 255, 255));
                DockShadow.Color = Colors.Black;
                DockShadow.Opacity = 0.32;
                DockShadow.BlurRadius = 16;

                // 1. 간소화 도크 테마
                TimerHeaderCapsule.Background = new SolidColorBrush(Color.FromArgb(18, 255, 255, 255));
                TimerHeaderCapsule.BorderBrush = new SolidColorBrush(Color.FromArgb(26, 255, 255, 255));
                TxtTimer.Foreground = Brushes.White;
                SessionBadgeCompact.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
                TxtSessionCompact.Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 224));
                MemberBadge.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
                TxtMember.Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 224));
                DockDivider.Background = new SolidColorBrush(Color.FromArgb(16, 255, 255, 255));

                BtnPlayPause.Background = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
                BtnPlayPause.BorderBrush = new SolidColorBrush(Color.FromArgb(42, 255, 255, 255));
                BtnPlayPause.Foreground = Brushes.White;
                BtnSkip.Foreground = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                BtnReset.Foreground = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                BtnSettings.Foreground = new SolidColorBrush(Color.FromRgb(235, 235, 235));

                // 2. 표준 도크 테마
                StdModeCapsule.Background = new SolidColorBrush(Color.FromArgb(18, 255, 255, 255));
                StdModeCapsule.BorderBrush = new SolidColorBrush(Color.FromArgb(26, 255, 255, 255));
                TxtTimerStd.Foreground = Brushes.White;
                SessionBadgeStd.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
                TxtSessionStd.Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 224));
                MemberBadgeStd.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
                TxtMemberStd.Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 224));
                ProgressStd.Background = new SolidColorBrush(Color.FromArgb(24, 255, 255, 255));
                DockDividerStd.Background = new SolidColorBrush(Color.FromArgb(16, 255, 255, 255));

                BtnPlayPauseStd.Background = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
                BtnPlayPauseStd.BorderBrush = new SolidColorBrush(Color.FromArgb(42, 255, 255, 255));
                BtnPlayPauseStd.Foreground = Brushes.White;
                BtnSkipStd.Foreground = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                BtnResetStd.Foreground = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                BtnSettingsStd.Foreground = new SolidColorBrush(Color.FromRgb(235, 235, 235));

                // 3. 원형 링 도크 테마
                RingModeCapsule.Background = new SolidColorBrush(Color.FromArgb(18, 255, 255, 255));
                RingModeCapsule.BorderBrush = new SolidColorBrush(Color.FromArgb(26, 255, 255, 255));
                RingTrack.Stroke = new SolidColorBrush(Color.FromArgb(28, 255, 255, 255));
                TxtTimerRing.Foreground = Brushes.White;
                TxtRingPercent.Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 164));
                TxtRingSession.Foreground = new SolidColorBrush(Color.FromRgb(190, 190, 195));

                BtnPlayPauseRing.Background = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
                BtnPlayPauseRing.BorderBrush = new SolidColorBrush(Color.FromArgb(42, 255, 255, 255));
                BtnPlayPauseRing.Foreground = Brushes.White;
                BtnSkipRing.Foreground = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                BtnResetRing.Foreground = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                BtnSettingsRing.Foreground = new SolidColorBrush(Color.FromRgb(235, 235, 235));

                RingDivider.Background = new SolidColorBrush(Color.FromArgb(16, 255, 255, 255));
                RingStatsCard.Background = new SolidColorBrush(Color.FromArgb(18, 255, 255, 255));
                RingStatsCard.BorderBrush = new SolidColorBrush(Color.FromArgb(24, 255, 255, 255));

                TxtStatTodayVal.Foreground = Brushes.White;
                TxtStatTotalVal.Foreground = Brushes.White;
                TxtStatTimeVal.Foreground = Brushes.White;
                TxtStatTodayLbl.Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 155));
                TxtStatTotalLbl.Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 155));
                TxtStatTimeLbl.Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 155));
            }
        }

        private void Timer_OnTick()
        {
            Dispatcher.Invoke(UpdateTimerUI);
        }

        private void Timer_OnStateChanged(PomodoroMode newMode)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateCharacterImage(animate: true);
                UpdateTimerUI();
                MemoryOptimizer.TrimMemory();
            });
        }

        private void UpdateTimerUI()
        {
            string timeStr = _timer.GetFormattedTime();
            TxtTimer.Text = timeStr;
            TxtTimerStd.Text = timeStr;
            TxtTimerRing.Text = timeStr;

            string playText = _timer.IsRunning 
                ? LocalizationManager.Get("Pause") 
                : LocalizationManager.Get("Start");
            BtnPlayPause.Content = playText;
            BtnPlayPauseStd.Content = playText;
            BtnPlayPauseRing.Content = playText;

            string skipText = LocalizationManager.Get("Skip");
            BtnSkip.Content = skipText;
            BtnSkipStd.Content = skipText;
            BtnSkipRing.Content = skipText;

            string resetTip = LocalizationManager.Get("ResetTooltip");
            BtnReset.ToolTip = resetTip;
            BtnResetStd.ToolTip = resetTip;
            BtnResetRing.ToolTip = resetTip;

            string settingsTip = LocalizationManager.Get("SettingsTooltip");
            BtnSettings.ToolTip = settingsTip;
            BtnSettingsStd.ToolTip = settingsTip;
            BtnSettingsRing.ToolTip = settingsTip;

            BtnStdFocus.Content = LocalizationManager.Get("Focus");
            BtnStdBreak.Content = LocalizationManager.Get("Break");
            BtnStdLongBreak.Content = LocalizationManager.Get("LongBreak");
            BtnRingFocus.Content = LocalizationManager.Get("Focus");
            BtnRingBreak.Content = LocalizationManager.Get("Break");
            BtnRingLongBreak.Content = LocalizationManager.Get("LongBreak");

            var currentMember = MemberInfo.GetById(_settings.Member);
            string memberName = LocalizationManager.IsEnglish ? currentMember.NameEn : currentMember.NameKo;
            TxtMember.Text = memberName;
            TxtMemberStd.Text = memberName;

            bool isLight = _settings.IsLightTheme;
            var memberCol = currentMember.GetThemeColor(isLight);
            var memberBrush = currentMember.GetThemeBrush(isLight);

            // 멤버 뱃지 스타일을 해당 멤버의 상징색에 맞게 적용
            byte mBgAlpha = isLight ? (byte)22 : (byte)38;
            byte mBorderAlpha = isLight ? (byte)55 : (byte)85;
            var memberBadgeBg = new SolidColorBrush(Color.FromArgb(mBgAlpha, memberCol.R, memberCol.G, memberCol.B));
            var memberBadgeBorder = new SolidColorBrush(Color.FromArgb(mBorderAlpha, memberCol.R, memberCol.G, memberCol.B));
            MemberBadge.Background = memberBadgeBg;
            MemberBadge.BorderBrush = memberBadgeBorder;
            MemberBadgeStd.Background = memberBadgeBg;
            MemberBadgeStd.BorderBrush = memberBadgeBorder;
            TxtMember.Foreground = memberBrush;
            TxtMemberStd.Foreground = memberBrush;

            int curSession = _timer.CurrentSessionInCycle;
            int totSession = _timer.TotalSessionsInCycle;
            TxtSessionCompact.Text = $"{curSession}/{totSession}";
            TxtSessionStd.Text = LocalizationManager.IsEnglish ? $"Session {curSession}/{totSession}" : $"세션 {curSession}/{totSession}";
            TxtRingSession.Text = LocalizationManager.IsEnglish ? $"Session {curSession}/{totSession}" : $"세션 {curSession} / {totSession}";

            double ratio = _timer.ProgressRatio;
            ProgressStd.Value = ratio;
            TxtRingPercent.Text = $"{_timer.ProgressPercent}%";

            Brush accentBrush;
            string modeName;

            switch (_timer.CurrentMode)
            {
                case PomodoroMode.Focus:
                    modeName = LocalizationManager.Get("Focus");
                    TxtMode.Text = modeName;
                    accentBrush = memberBrush;
                    BadgeDot.Fill = accentBrush;
                    byte fBgAlpha = isLight ? (byte)22 : (byte)45;
                    byte fBorderAlpha = isLight ? (byte)55 : (byte)95;
                    ModeBadge.Background = new SolidColorBrush(Color.FromArgb(fBgAlpha, memberCol.R, memberCol.G, memberCol.B));
                    ModeBadge.BorderBrush = new SolidColorBrush(Color.FromArgb(fBorderAlpha, memberCol.R, memberCol.G, memberCol.B));
                    TxtMode.Foreground = isLight ? (currentMember.Id == "liv" ? Brushes.Black : accentBrush) : Brushes.White;
                    break;
                case PomodoroMode.Break:
                    modeName = LocalizationManager.Get("Break");
                    TxtMode.Text = modeName;
                    if (isLight)
                    {
                        accentBrush = new SolidColorBrush(Color.FromRgb(40, 205, 65));
                        BadgeDot.Fill = accentBrush;
                        ModeBadge.Background = new SolidColorBrush(Color.FromArgb(24, 40, 205, 65));
                        ModeBadge.BorderBrush = new SolidColorBrush(Color.FromArgb(50, 40, 205, 65));
                        TxtMode.Foreground = new SolidColorBrush(Color.FromRgb(30, 142, 62));
                    }
                    else
                    {
                        accentBrush = new SolidColorBrush(Color.FromRgb(52, 199, 89));
                        BadgeDot.Fill = accentBrush;
                        ModeBadge.Background = new SolidColorBrush(Color.FromArgb(45, 52, 199, 89));
                        ModeBadge.BorderBrush = new SolidColorBrush(Color.FromArgb(90, 52, 199, 89));
                        TxtMode.Foreground = Brushes.White;
                    }
                    break;
                case PomodoroMode.LongBreak:
                default:
                    modeName = LocalizationManager.Get("LongBreak");
                    TxtMode.Text = modeName;
                    if (isLight)
                    {
                        accentBrush = new SolidColorBrush(Color.FromRgb(0, 122, 255));
                        BadgeDot.Fill = accentBrush;
                        ModeBadge.Background = new SolidColorBrush(Color.FromArgb(24, 0, 122, 255));
                        ModeBadge.BorderBrush = new SolidColorBrush(Color.FromArgb(50, 0, 122, 255));
                        TxtMode.Foreground = new SolidColorBrush(Color.FromRgb(0, 85, 179));
                    }
                    else
                    {
                        accentBrush = new SolidColorBrush(Color.FromRgb(10, 132, 255));
                        BadgeDot.Fill = accentBrush;
                        ModeBadge.Background = new SolidColorBrush(Color.FromArgb(40, 10, 132, 255));
                        ModeBadge.BorderBrush = new SolidColorBrush(Color.FromArgb(90, 10, 132, 255));
                        TxtMode.Foreground = Brushes.White;
                    }
                    break;
            }

            ProgressStd.Foreground = accentBrush;
            TxtRingMode.Text = modeName;
            TxtRingMode.Foreground = accentBrush;

            UpdateRingArc(ratio, accentBrush);
            UpdateModeTabHighlight(_timer.CurrentMode, isLight, accentBrush);

            // 원형 도크 세션 도트
            var dotInactive = isLight
                ? new SolidColorBrush(Color.FromArgb(36, 0, 0, 0))
                : new SolidColorBrush(Color.FromArgb(38, 255, 255, 255));
            RingDot1.Fill = curSession >= 1 ? accentBrush : dotInactive;
            RingDot2.Fill = curSession >= 2 ? accentBrush : dotInactive;
            RingDot3.Fill = curSession >= 3 ? accentBrush : dotInactive;
            RingDot4.Fill = curSession >= 4 ? accentBrush : dotInactive;

            // 원형 도크 오늘 / 누적 통계
            _settings.CheckAndResetDailyStats();
            TxtStatTodayVal.Text = _settings.TodayCompletedSessions.ToString();
            TxtStatTotalVal.Text = _settings.TotalCompletedSessions.ToString();
            int focusMins = _settings.TodayFocusMinutes;
            if (LocalizationManager.IsEnglish)
            {
                TxtStatTimeVal.Text = focusMins >= 60 ? $"{focusMins / 60}h {focusMins % 60}m" : $"{focusMins}m";
            }
            else
            {
                TxtStatTimeVal.Text = focusMins >= 60 ? $"{focusMins / 60}시간 {focusMins % 60}분" : $"{focusMins}분";
            }
            TxtStatTodayLbl.Text = LocalizationManager.Get("StatToday");
            TxtStatTotalLbl.Text = LocalizationManager.Get("StatTotal");
            TxtStatTimeLbl.Text = LocalizationManager.Get("StatTime");
        }

        private void UpdateModeTabHighlight(PomodoroMode mode, bool isLight, Brush activeBrush)
        {
            void StyleTab(Button btn, bool isActive)
            {
                if (isActive)
                {
                    btn.Background = isLight ? new SolidColorBrush(Color.FromArgb(28, 0, 122, 255)) : new SolidColorBrush(Color.FromArgb(42, 255, 255, 255));
                    btn.BorderBrush = activeBrush;
                    btn.Foreground = isLight ? activeBrush : Brushes.White;
                }
                else
                {
                    btn.Background = Brushes.Transparent;
                    btn.BorderBrush = Brushes.Transparent;
                    btn.Foreground = isLight ? new SolidColorBrush(Color.FromRgb(120, 120, 128)) : new SolidColorBrush(Color.FromRgb(160, 160, 164));
                }
            }

            StyleTab(BtnStdFocus, mode == PomodoroMode.Focus);
            StyleTab(BtnStdBreak, mode == PomodoroMode.Break);
            StyleTab(BtnStdLongBreak, mode == PomodoroMode.LongBreak);

            StyleTab(BtnRingFocus, mode == PomodoroMode.Focus);
            StyleTab(BtnRingBreak, mode == PomodoroMode.Break);
            StyleTab(BtnRingLongBreak, mode == PomodoroMode.LongBreak);
        }

        private void UpdateRingArc(double progress, Brush strokeBrush)
        {
            double cx = 80.0;
            double cy = 80.0;
            double radius = 64.5;

            ArcRingProgress.Stroke = strokeBrush;

            if (progress <= 0.001)
            {
                ArcRingProgress.Data = null;
                return;
            }

            if (progress >= 0.999)
            {
                ArcRingProgress.Data = new EllipseGeometry(new Point(cx, cy), radius, radius);
                return;
            }

            double angle = progress * 360.0;
            double angleRad = (angle - 90.0) * Math.PI / 180.0;
            Point startPoint = new Point(cx, cy - radius);
            Point endPoint = new Point(cx + radius * Math.Cos(angleRad), cy + radius * Math.Sin(angleRad));
            bool isLargeArc = angle > 180.0;

            var figure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false,
                IsFilled = false
            };
            figure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                IsLargeArc = isLargeArc,
                SweepDirection = SweepDirection.Clockwise
            });

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            ArcRingProgress.Data = geometry;
        }

        private void BtnStdFocus_Click(object sender, RoutedEventArgs e)
        {
            _timer.SetMode(PomodoroMode.Focus);
        }

        private void BtnStdBreak_Click(object sender, RoutedEventArgs e)
        {
            _timer.SetMode(PomodoroMode.Break);
        }

        private void BtnStdLongBreak_Click(object sender, RoutedEventArgs e)
        {
            _timer.SetMode(PomodoroMode.LongBreak);
        }

        private void UpdateCharacterImage(bool animate = true)
        {
            bool isFocus = _timer.CurrentMode == PomodoroMode.Focus;
            var newImg = ImageHelper.LoadMemberImage(_settings.Member, isFocus);

            if (newImg != null)
            {
                CharacterImage.Source = newImg;
                var targetOpacity = _settings.CharacterOpacity;

                if (animate)
                {
                    // 부드러운 페이드 인 애니메이션 (목표 불투명도로 페이드)
                    var fadeAnim = new DoubleAnimation(Math.Min(0.2, targetOpacity * 0.3), targetOpacity, TimeSpan.FromMilliseconds(250));
                    CharacterImage.BeginAnimation(OpacityProperty, fadeAnim);
                }
                else
                {
                    CharacterImage.BeginAnimation(OpacityProperty, null);
                    CharacterImage.Opacity = targetOpacity;
                }
            }
        }

        private void UpdateOverlayVisibility(bool isMouseOver)
        {
            double targetOpacity = (_settings.AlwaysShowTime || isMouseOver) ? 1.0 : 0.0;
            var anim = new DoubleAnimation(TimerOverlay.Opacity, targetOpacity, TimeSpan.FromMilliseconds(200));
            TimerOverlay.BeginAnimation(OpacityProperty, anim);
        }

        private void SetupContextMenu()
        {
            var menuStyle = (Style)FindResource("FluentContextMenu");
            var itemStyle = (Style)FindResource("FluentMenuItem");
            var sepStyle = (Style)FindResource("FluentMenuSeparator");

            bool isLight = _settings.IsLightTheme;
            var menu = new ContextMenu { Style = menuStyle };

            if (isLight)
            {
                menu.Background = new SolidColorBrush(Color.FromArgb(248, 255, 255, 255));
                menu.BorderBrush = new SolidColorBrush(Color.FromArgb(32, 0, 0, 0));
                menu.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
            }
            else
            {
                menu.Background = new SolidColorBrush(Color.FromArgb(242, 26, 26, 28));
                menu.BorderBrush = new SolidColorBrush(Color.FromArgb(25, 255, 255, 255));
                menu.Foreground = new SolidColorBrush(Color.FromRgb(238, 238, 238));
            }

            MenuItem CreateItem(string header, RoutedEventHandler onClick, bool isCheckable = false, bool isChecked = false)
            {
                var itm = new MenuItem
                {
                    Header = header,
                    Style = itemStyle,
                    IsCheckable = isCheckable,
                    IsChecked = isChecked,
                    Foreground = isLight ? new SolidColorBrush(Color.FromRgb(29, 29, 31)) : new SolidColorBrush(Color.FromRgb(238, 238, 238))
                };
                itm.Click += onClick;
                return itm;
            }

            Separator CreateSeparator() => new Separator
            {
                Style = sepStyle,
                Background = isLight ? new SolidColorBrush(Color.FromArgb(18, 0, 0, 0)) : new SolidColorBrush(Color.FromArgb(18, 255, 255, 255))
            };

            // 1. 시작 / 일시정지
            menu.Items.Add(CreateItem(_timer.IsRunning ? LocalizationManager.Get("MenuPause") : LocalizationManager.Get("MenuStart"), (s, e) => _timer.Toggle()));

            // 2. 다음 단계 스킵
            menu.Items.Add(CreateItem(LocalizationManager.Get("MenuSkip"), (s, e) => _timer.SkipNext()));

            // 3. 리셋
            menu.Items.Add(CreateItem(LocalizationManager.Get("MenuReset"), (s, e) => _timer.Reset()));

            menu.Items.Add(CreateSeparator());

            // 4. 멤버 선택 서브메뉴
            var memberSubMenu = new MenuItem
            {
                Header = LocalizationManager.Get("MenuSelectMember"),
                Style = itemStyle,
                Foreground = isLight ? new SolidColorBrush(Color.FromRgb(29, 29, 31)) : new SolidColorBrush(Color.FromRgb(238, 238, 238))
            };
            foreach (var member in MemberInfo.Members)
            {
                var memberItem = new MenuItem
                {
                    Header = $"{member.NameKo} ({member.NameEn})",
                    Style = itemStyle,
                    IsCheckable = true,
                    IsChecked = _settings.Member.Equals(member.Id, StringComparison.OrdinalIgnoreCase),
                    Foreground = isLight ? new SolidColorBrush(Color.FromRgb(29, 29, 31)) : new SolidColorBrush(Color.FromRgb(238, 238, 238))
                };
                memberItem.Click += (s, e) =>
                {
                    _settings.Member = member.Id;
                    _settings.Save();
                    ApplySettings();
                };
                memberSubMenu.Items.Add(memberItem);
            }
            menu.Items.Add(memberSubMenu);

            // 4-2. 타이머 스타일 서브메뉴
            var styleSubMenu = new MenuItem
            {
                Header = LocalizationManager.Get("MenuTimerStyle"),
                Style = itemStyle,
                Foreground = isLight ? new SolidColorBrush(Color.FromRgb(29, 29, 31)) : new SolidColorBrush(Color.FromRgb(238, 238, 238))
            };

            string curStyle = string.IsNullOrEmpty(_settings.TimerStyle) ? "compact" : _settings.TimerStyle.ToLowerInvariant();

            void AddStyleItem(string styleKey, string styleVal)
            {
                var sItem = new MenuItem
                {
                    Header = LocalizationManager.Get(styleKey),
                    Style = itemStyle,
                    IsCheckable = true,
                    IsChecked = curStyle == styleVal,
                    Foreground = isLight ? new SolidColorBrush(Color.FromRgb(29, 29, 31)) : new SolidColorBrush(Color.FromRgb(238, 238, 238))
                };
                sItem.Click += (s, e) =>
                {
                    _settings.TimerStyle = styleVal;
                    _settings.Save();
                    ApplySettings();
                };
                styleSubMenu.Items.Add(sItem);
            }

            AddStyleItem("TimerStyleCompact", "compact");
            AddStyleItem("TimerStyleStandard", "standard");
            AddStyleItem("TimerStyleRing", "ring");
            menu.Items.Add(styleSubMenu);

            menu.Items.Add(CreateSeparator());

            // 5. 항상 위에 표시 토글
            menu.Items.Add(CreateItem(LocalizationManager.Get("MenuAlwaysOnTop"), (s, e) =>
            {
                if (s is MenuItem mi)
                {
                    _settings.AlwaysOnTop = mi.IsChecked;
                    _settings.Save();
                    ApplySettings();
                }
            }, isCheckable: true, isChecked: _settings.AlwaysOnTop));

            // 6. 시간 항상 표시 토글
            menu.Items.Add(CreateItem(LocalizationManager.Get("MenuAlwaysShowTime"), (s, e) =>
            {
                if (s is MenuItem mi)
                {
                    _settings.AlwaysShowTime = mi.IsChecked;
                    _settings.Save();
                    ApplySettings();
                }
            }, isCheckable: true, isChecked: _settings.AlwaysShowTime));

            // 7. 이미지 폴더 열기
            menu.Items.Add(CreateItem(LocalizationManager.Get("MenuOpenImages"), (s, e) => ImageHelper.OpenImagesFolder()));

            // 7-2. 음성 폴더 열기
            menu.Items.Add(CreateItem(LocalizationManager.Get("MenuOpenSounds"), (s, e) => SoundManager.OpenSoundsFolder()));

            // 7-3. 테마 토글 (화이트 / 블랙)
            menu.Items.Add(CreateItem(LocalizationManager.Get("MenuToggleTheme"), (s, e) =>
            {
                _settings.Theme = _settings.IsLightTheme ? "dark" : "light";
                _settings.Save();
                ApplySettings();
            }));

            // 8. 상세 설정창 열기
            menu.Items.Add(CreateItem(LocalizationManager.Get("MenuSettings"), (s, e) => OpenSettingsWindow()));

            menu.Items.Add(CreateSeparator());

            // 9. 종료
            menu.Items.Add(CreateItem(LocalizationManager.Get("MenuExit"), (s, e) => Application.Current.Shutdown()));

            ContextMenu = menu;
        }

        public void OpenSettingsWindow()
        {
            if (_settingsWindow == null || !_settingsWindow.IsLoaded)
            {
                _settingsWindow = new SettingsWindow(_settings, this);
                _settingsWindow.Owner = this;
                _settingsWindow.Closed += (s, e) =>
                {
                    _settingsWindow = null;
                    MemoryOptimizer.TrimMemory();
                };
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Activate();
            }
        }

        #region UI Events

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CharacterContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // 더블 클릭 시 타이머 토글
                _timer.Toggle();
                e.Handled = true;
            }
        }

        private void CharacterContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateOverlayVisibility(isMouseOver: true);
        }

        private void CharacterContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateOverlayVisibility(isMouseOver: false);
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            _timer.Toggle();
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            _timer.SkipNext();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _timer.Reset();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenSettingsWindow();
        }

        #endregion
    }
}