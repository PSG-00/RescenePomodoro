using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace radiant_noether
{
    public partial class SettingsWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly MainWindow _mainWindow;
        private string _selectedMember;
        private string _selectedLanguage;
        private string _selectedTheme;
        private string _selectedTimerStyle;
        private bool _isInitializing = true;

        // 원본 설정 스냅샷 (미저장 닫기 시 원복용)
        private readonly string _origMember;
        private readonly string _origLanguage;
        private readonly string _origTheme;
        private readonly string _origTimerStyle;
        private readonly double _origSize;
        private readonly double _origOpacity;
        private readonly bool _origAlwaysOnTop;
        private readonly bool _origAlwaysShowTime;
        private readonly bool _origPlaySound;
        private readonly int _origFocus;
        private readonly int _origBreak;
        private readonly int _origLongBreak;
        private bool _hasSaved = false;

        public SettingsWindow(AppSettings settings, MainWindow mainWindow)
        {
            InitializeComponent();
            _settings = settings;
            _mainWindow = mainWindow;
            _selectedMember = _settings.Member;
            _selectedLanguage = string.IsNullOrEmpty(_settings.Language) ? "ko" : _settings.Language;
            _selectedTheme = string.IsNullOrEmpty(_settings.Theme) ? "dark" : _settings.Theme;
            _selectedTimerStyle = string.IsNullOrEmpty(_settings.TimerStyle) ? "compact" : _settings.TimerStyle;

            // 스냅샷 값 기록
            _origMember = _selectedMember;
            _origLanguage = _selectedLanguage;
            _origTheme = _selectedTheme;
            _origTimerStyle = _selectedTimerStyle;
            _origSize = _settings.CharacterSize;
            _origOpacity = _settings.CharacterOpacity;
            _origAlwaysOnTop = _settings.AlwaysOnTop;
            _origAlwaysShowTime = _settings.AlwaysShowTime;
            _origPlaySound = _settings.PlaySound;
            _origFocus = _settings.FocusMinutes;
            _origBreak = _settings.BreakMinutes;
            _origLongBreak = _settings.LongBreakMinutes;

            LoadSettingsToUI();
            _isInitializing = false;
        }

        private void LoadSettingsToUI()
        {
            // 0. 테마 라디오 설정
            RadioThemeDark.IsChecked = _selectedTheme.Equals("dark", StringComparison.OrdinalIgnoreCase);
            RadioThemeLight.IsChecked = _selectedTheme.Equals("light", StringComparison.OrdinalIgnoreCase);

            // 1. 언어 설정 라디오
            RadioLangKo.IsChecked = _selectedLanguage.Equals("ko", StringComparison.OrdinalIgnoreCase);
            RadioLangEn.IsChecked = _selectedLanguage.Equals("en", StringComparison.OrdinalIgnoreCase);

            // 1-2. 타이머 위젯 스타일 라디오
            RadioStyleCompact.IsChecked = _selectedTimerStyle.Equals("compact", StringComparison.OrdinalIgnoreCase);
            RadioStyleStandard.IsChecked = _selectedTimerStyle.Equals("standard", StringComparison.OrdinalIgnoreCase);
            RadioStyleRing.IsChecked = _selectedTimerStyle.Equals("ring", StringComparison.OrdinalIgnoreCase);

            LocalizationManager.CurrentLanguage = _selectedLanguage;
            ApplyLocalizationTexts();

            // 2. 테마 시각화 적용
            ApplyThemeToWindow(_selectedTheme.Equals("light", StringComparison.OrdinalIgnoreCase));
            UpdateSegmentRadioVisuals();

            // 3. 멤버 라디오 카드 빌드
            BuildMemberCards();

            // 4. 슬라이더 & 숫자 입력란 값 설정
            SliderFocus.Value = _settings.FocusMinutes;
            SliderBreak.Value = _settings.BreakMinutes;
            SliderLongBreak.Value = _settings.LongBreakMinutes;
            SliderSize.Value = _settings.CharacterSize;
            SliderOpacity.Value = _settings.CharacterOpacity;

            TxtFocusInput.Text = _settings.FocusMinutes.ToString();
            TxtBreakInput.Text = _settings.BreakMinutes.ToString();
            TxtLongBreakInput.Text = _settings.LongBreakMinutes.ToString();

            TxtSizeVal.Text = $"{(int)_settings.CharacterSize}px";
            TxtOpacityVal.Text = $"{(int)(_settings.CharacterOpacity * 100)}%";

            // 5. 토글 체크박스 설정
            ChkAlwaysOnTop.IsChecked = _settings.AlwaysOnTop;
            ChkAlwaysShowTime.IsChecked = _settings.AlwaysShowTime;
            ChkPlaySound.IsChecked = _settings.PlaySound;
        }

        private void ApplyLocalizationTexts()
        {
            Title = LocalizationManager.Get("SettingsTitle");
            TxtHeaderTitle.Text = LocalizationManager.Get("SettingsHeaderTitle");
            TxtHeaderSubtitle.Text = LocalizationManager.Get("SettingsHeaderSubtitle");

            LblSectionTheme.Text = LocalizationManager.Get("SectionTheme");
            LblThemeDesc.Text = LocalizationManager.Get("ThemeDesc");
            RadioThemeDark.Content = LocalizationManager.Get("ThemeDark");
            RadioThemeLight.Content = LocalizationManager.Get("ThemeLight");

            LblSectionLanguage.Text = LocalizationManager.Get("SectionLanguage");
            LblLanguageDesc.Text = LocalizationManager.IsEnglish 
                ? "Select display language applied across the application." 
                : "프로그램 전체에 적용할 표시 언어를 선택하세요.";

            RadioLangKo.Content = LocalizationManager.Get("LangKo");
            RadioLangEn.Content = LocalizationManager.Get("LangEn");

            LblSectionTimerStyle.Text = LocalizationManager.Get("SectionTimerStyle");
            LblTimerStyleDesc.Text = LocalizationManager.Get("TimerStyleDesc");
            RadioStyleCompact.Content = LocalizationManager.Get("TimerStyleCompact");
            RadioStyleStandard.Content = LocalizationManager.Get("TimerStyleStandard");
            RadioStyleRing.Content = LocalizationManager.Get("TimerStyleRing");
            UpdateTimerStyleDetailText();

            LblSectionMember.Text = LocalizationManager.Get("SectionMember");
            LblMemberDesc.Text = LocalizationManager.Get("MemberDesc");

            LblSectionTimer.Text = LocalizationManager.Get("SectionTimer");
            LblCardFocusTitle.Text = LocalizationManager.Get("CardFocusTitle");
            LblCardFocusDesc.Text = LocalizationManager.Get("CardFocusDesc");
            LblCardBreakTitle.Text = LocalizationManager.Get("CardBreakTitle");
            LblCardBreakDesc.Text = LocalizationManager.Get("CardBreakDesc");
            LblCardLongBreakTitle.Text = LocalizationManager.Get("CardLongBreakTitle");
            LblCardLongBreakDesc.Text = LocalizationManager.Get("CardLongBreakDesc");

            LblSectionAppearance.Text = LocalizationManager.Get("SectionAppearance");
            LblCardSizeTitle.Text = LocalizationManager.Get("CardSizeTitle");
            LblCardSizeDesc.Text = LocalizationManager.Get("CardSizeDesc");
            LblCardOpacityTitle.Text = LocalizationManager.Get("CardOpacityTitle");
            LblCardOpacityDesc.Text = LocalizationManager.Get("CardOpacityDesc");
            LblCardAlwaysOnTopTitle.Text = LocalizationManager.Get("CardAlwaysOnTopTitle");
            LblCardAlwaysOnTopDesc.Text = LocalizationManager.Get("CardAlwaysOnTopDesc");
            LblCardAlwaysShowTimeTitle.Text = LocalizationManager.Get("CardAlwaysShowTimeTitle");
            LblCardAlwaysShowTimeDesc.Text = LocalizationManager.Get("CardAlwaysShowTimeDesc");
            LblCardSoundTitle.Text = LocalizationManager.Get("CardSoundTitle");
            LblCardSoundDesc.Text = LocalizationManager.Get("CardSoundDesc");

            BtnImages.Content = LocalizationManager.Get("BtnImages");
            BtnImages.ToolTip = LocalizationManager.Get("BtnImagesTooltip");
            BtnSounds.Content = LocalizationManager.Get("BtnSounds");
            BtnSounds.ToolTip = LocalizationManager.Get("BtnSoundsTooltip");
            BtnSave.Content = LocalizationManager.Get("BtnSave");

            string unit = LocalizationManager.Get("UnitMinutes");
            LblFocusUnit.Text = unit;
            LblBreakUnit.Text = unit;
            LblLongBreakUnit.Text = unit;
        }

        private void ApplyThemeToWindow(bool isLight)
        {
            if (isLight)
            {
                // Dynamic Accent & Toggle Brushes (스마트폰 iOS / Android 시그니처 블루)
                var accentBrush = new SolidColorBrush(Color.FromRgb(0, 122, 255)); // #007AFF
                var accentHoverBrush = new SolidColorBrush(Color.FromRgb(26, 137, 255));
                var accentPressedBrush = new SolidColorBrush(Color.FromRgb(0, 106, 224));
                var toggleTrackOff = new SolidColorBrush(Color.FromRgb(229, 229, 234)); // #E5E5EA
                var toggleBorderOff = new SolidColorBrush(Color.FromRgb(209, 209, 214)); // #D1D1D6
                var toggleThumbOff = Brushes.White;

                Resources["AccentBrush"] = accentBrush;
                Resources["AccentHoverBrush"] = accentHoverBrush;
                Resources["AccentPressedBrush"] = accentPressedBrush;
                Resources["ToggleTrackOffBrush"] = toggleTrackOff;
                Resources["ToggleBorderOffBrush"] = toggleBorderOff;
                Resources["ToggleThumbOffBrush"] = toggleThumbOff;

                BtnSave.Background = accentBrush;
                BtnSave.BorderBrush = accentBrush;
                BtnSave.Foreground = Brushes.White;

                TxtFocusInput.SelectionBrush = accentBrush;
                TxtBreakInput.SelectionBrush = accentBrush;
                TxtLongBreakInput.SelectionBrush = accentBrush;

                // Apple iOS / Samsung One UI 스타일 클린 화이트
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 247));
                Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));

                HeaderBorder.Background = Brushes.White;
                HeaderBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 234));
                TxtHeaderTitle.Foreground = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                TxtHeaderSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(134, 134, 139));

                var sectionHeaderBrush = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                LblSectionTheme.Foreground = sectionHeaderBrush;
                LblSectionLanguage.Foreground = sectionHeaderBrush;
                LblSectionTimerStyle.Foreground = sectionHeaderBrush;
                LblSectionMember.Foreground = sectionHeaderBrush;
                LblSectionTimer.Foreground = sectionHeaderBrush;
                LblSectionAppearance.Foreground = sectionHeaderBrush;

                var cardBg = Brushes.White;
                var cardBorder = new SolidColorBrush(Color.FromRgb(229, 229, 234));

                var cards = new[] { CardTheme, CardLanguage, CardTimerStyle, CardMember, CardFocus, CardBreak, CardLongBreak, CardSize, CardOpacity, CardAlwaysOnTop, CardAlwaysShowTime, CardSound };
                foreach (var card in cards)
                {
                    card.Background = cardBg;
                    card.BorderBrush = cardBorder;
                }

                var titleBrush = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                var cardTitles = new[] { LblCardFocusTitle, LblCardBreakTitle, LblCardLongBreakTitle, LblCardSizeTitle, LblCardOpacityTitle, LblCardAlwaysOnTopTitle, LblCardAlwaysShowTimeTitle, LblCardSoundTitle };
                foreach (var title in cardTitles)
                {
                    title.Foreground = titleBrush;
                }

                var descBrush = new SolidColorBrush(Color.FromRgb(110, 110, 115));
                var descs = new[] { LblThemeDesc, LblLanguageDesc, LblTimerStyleDesc, LblTimerStyleDetail, LblMemberDesc, LblCardFocusDesc, LblCardBreakDesc, LblCardLongBreakDesc, LblCardSizeDesc, LblCardOpacityDesc, LblCardAlwaysOnTopDesc, LblCardAlwaysShowTimeDesc, LblCardSoundDesc };
                foreach (var desc in descs)
                {
                    desc.Foreground = descBrush;
                }

                var badgeBg = new SolidColorBrush(Color.FromRgb(242, 242, 247));
                var badgeBorder = new SolidColorBrush(Color.FromRgb(218, 218, 222));
                var badges = new[] { BadgeFocus, BadgeBreak, BadgeLongBreak, BadgeSize, BadgeOpacity };
                foreach (var badge in badges)
                {
                    badge.Background = badgeBg;
                    badge.BorderBrush = badgeBorder;
                }

                var inputBrush = new SolidColorBrush(Color.FromRgb(29, 29, 31));
                TxtFocusInput.Foreground = inputBrush;
                TxtFocusInput.CaretBrush = inputBrush;
                TxtBreakInput.Foreground = inputBrush;
                TxtBreakInput.CaretBrush = inputBrush;
                TxtLongBreakInput.Foreground = inputBrush;
                TxtLongBreakInput.CaretBrush = inputBrush;

                var unitBrush = new SolidColorBrush(Color.FromRgb(142, 142, 147));
                LblFocusUnit.Foreground = unitBrush;
                LblBreakUnit.Foreground = unitBrush;
                LblLongBreakUnit.Foreground = unitBrush;

                TxtSizeVal.Foreground = inputBrush;
                TxtOpacityVal.Foreground = inputBrush;

                FooterBorder.Background = Brushes.White;
                FooterBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 234));

                var btnNormalBg = new SolidColorBrush(Color.FromRgb(242, 242, 247));
                var btnNormalBorder = new SolidColorBrush(Color.FromRgb(218, 218, 222));
                var btnNormalFg = new SolidColorBrush(Color.FromRgb(29, 29, 31));

                BtnImages.Background = btnNormalBg;
                BtnImages.BorderBrush = btnNormalBorder;
                BtnImages.Foreground = btnNormalFg;

                BtnSounds.Background = btnNormalBg;
                BtnSounds.BorderBrush = btnNormalBorder;
                BtnSounds.Foreground = btnNormalFg;
            }
            else
            {
                // Dynamic Accent & Toggle Brushes (정제된 딥 크림슨/로즈 & 다크 트랙)
                var accentBrush = new SolidColorBrush(Color.FromRgb(224, 64, 96)); // #E04060
                var accentHoverBrush = new SolidColorBrush(Color.FromRgb(235, 85, 115));
                var accentPressedBrush = new SolidColorBrush(Color.FromRgb(204, 53, 85));
                var toggleTrackOff = new SolidColorBrush(Color.FromRgb(58, 58, 60)); // #3A3A3C
                var toggleBorderOff = new SolidColorBrush(Color.FromRgb(72, 72, 74)); // #48484A
                var toggleThumbOff = new SolidColorBrush(Color.FromRgb(209, 209, 214));

                Resources["AccentBrush"] = accentBrush;
                Resources["AccentHoverBrush"] = accentHoverBrush;
                Resources["AccentPressedBrush"] = accentPressedBrush;
                Resources["ToggleTrackOffBrush"] = toggleTrackOff;
                Resources["ToggleBorderOffBrush"] = toggleBorderOff;
                Resources["ToggleThumbOffBrush"] = toggleThumbOff;

                BtnSave.Background = accentBrush;
                BtnSave.BorderBrush = accentBrush;
                BtnSave.Foreground = Brushes.White;

                TxtFocusInput.SelectionBrush = accentBrush;
                TxtBreakInput.SelectionBrush = accentBrush;
                TxtLongBreakInput.SelectionBrush = accentBrush;

                // 정제된 모던 다크 모드 (OLED 블랙)
                Background = new SolidColorBrush(Color.FromRgb(24, 24, 26));
                Foreground = new SolidColorBrush(Color.FromRgb(238, 238, 238));

                HeaderBorder.Background = new SolidColorBrush(Color.FromRgb(28, 28, 30));
                HeaderBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(22, 255, 255, 255));
                TxtHeaderTitle.Foreground = Brushes.White;
                TxtHeaderSubtitle.Foreground = new SolidColorBrush(Color.FromRgb(152, 152, 159));

                var sectionHeaderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224));
                LblSectionTheme.Foreground = sectionHeaderBrush;
                LblSectionLanguage.Foreground = sectionHeaderBrush;
                LblSectionTimerStyle.Foreground = sectionHeaderBrush;
                LblSectionMember.Foreground = sectionHeaderBrush;
                LblSectionTimer.Foreground = sectionHeaderBrush;
                LblSectionAppearance.Foreground = sectionHeaderBrush;

                var cardBg = new SolidColorBrush(Color.FromRgb(36, 36, 38));
                var cardBorder = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));

                var cards = new[] { CardTheme, CardLanguage, CardTimerStyle, CardMember, CardFocus, CardBreak, CardLongBreak, CardSize, CardOpacity, CardAlwaysOnTop, CardAlwaysShowTime, CardSound };
                foreach (var card in cards)
                {
                    card.Background = cardBg;
                    card.BorderBrush = cardBorder;
                }

                var cardTitles = new[] { LblCardFocusTitle, LblCardBreakTitle, LblCardLongBreakTitle, LblCardSizeTitle, LblCardOpacityTitle, LblCardAlwaysOnTopTitle, LblCardAlwaysShowTimeTitle, LblCardSoundTitle };
                foreach (var title in cardTitles)
                {
                    title.Foreground = Brushes.White;
                }

                var descBrush = new SolidColorBrush(Color.FromRgb(152, 152, 159));
                var descs = new[] { LblThemeDesc, LblLanguageDesc, LblTimerStyleDesc, LblTimerStyleDetail, LblMemberDesc, LblCardFocusDesc, LblCardBreakDesc, LblCardLongBreakDesc, LblCardSizeDesc, LblCardOpacityDesc, LblCardAlwaysOnTopDesc, LblCardAlwaysShowTimeDesc, LblCardSoundDesc };
                foreach (var desc in descs)
                {
                    desc.Foreground = descBrush;
                }

                var badgeBg = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
                var badgeBorder = new SolidColorBrush(Color.FromArgb(36, 255, 255, 255));
                var badges = new[] { BadgeFocus, BadgeBreak, BadgeLongBreak, BadgeSize, BadgeOpacity };
                foreach (var badge in badges)
                {
                    badge.Background = badgeBg;
                    badge.BorderBrush = badgeBorder;
                }

                var inputBrush = Brushes.White;
                TxtFocusInput.Foreground = inputBrush;
                TxtFocusInput.CaretBrush = inputBrush;
                TxtBreakInput.Foreground = inputBrush;
                TxtBreakInput.CaretBrush = inputBrush;
                TxtLongBreakInput.Foreground = inputBrush;
                TxtLongBreakInput.CaretBrush = inputBrush;

                var unitBrush = new SolidColorBrush(Color.FromRgb(160, 160, 160));
                LblFocusUnit.Foreground = unitBrush;
                LblBreakUnit.Foreground = unitBrush;
                LblLongBreakUnit.Foreground = unitBrush;

                TxtSizeVal.Foreground = inputBrush;
                TxtOpacityVal.Foreground = inputBrush;

                FooterBorder.Background = new SolidColorBrush(Color.FromRgb(28, 28, 30));
                FooterBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(22, 255, 255, 255));

                var btnNormalBg = new SolidColorBrush(Color.FromRgb(44, 44, 46));
                var btnNormalBorder = new SolidColorBrush(Color.FromArgb(34, 255, 255, 255));
                var btnNormalFg = new SolidColorBrush(Color.FromRgb(238, 238, 238));

                BtnImages.Background = btnNormalBg;
                BtnImages.BorderBrush = btnNormalBorder;
                BtnImages.Foreground = btnNormalFg;

                BtnSounds.Background = btnNormalBg;
                BtnSounds.BorderBrush = btnNormalBorder;
                BtnSounds.Foreground = btnNormalFg;
            }

            // 토글 스위치 비주얼 리프레시
            var toggleStyle = (Style)FindResource("FluentToggleSwitch");
            ChkAlwaysOnTop.Style = null;
            ChkAlwaysOnTop.Style = toggleStyle;
            ChkAlwaysShowTime.Style = null;
            ChkAlwaysShowTime.Style = toggleStyle;
            ChkPlaySound.Style = null;
            ChkPlaySound.Style = toggleStyle;
        }

        private void UpdateSegmentRadioVisuals()
        {
            bool isLight = _selectedTheme.Equals("light", StringComparison.OrdinalIgnoreCase);

            StyleSegmentRadio(RadioThemeDark, isLight);
            StyleSegmentRadio(RadioThemeLight, isLight);
            StyleSegmentRadio(RadioLangKo, isLight);
            StyleSegmentRadio(RadioLangEn, isLight);
            StyleSegmentRadio(RadioStyleCompact, isLight);
            StyleSegmentRadio(RadioStyleStandard, isLight);
            StyleSegmentRadio(RadioStyleRing, isLight);
        }

        private void StyleSegmentRadio(RadioButton rb, bool isLight)
        {
            bool isChecked = rb.IsChecked == true;
            if (isLight)
            {
                if (isChecked)
                {
                    rb.Background = Brushes.White;
                    rb.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 122, 255));
                    rb.BorderThickness = new Thickness(1.5);
                    rb.Foreground = new SolidColorBrush(Color.FromRgb(0, 122, 255));
                }
                else
                {
                    rb.Background = new SolidColorBrush(Color.FromRgb(242, 242, 247));
                    rb.BorderBrush = new SolidColorBrush(Color.FromRgb(218, 218, 222));
                    rb.BorderThickness = new Thickness(1);
                    rb.Foreground = new SolidColorBrush(Color.FromRgb(110, 110, 115));
                }
            }
            else
            {
                if (isChecked)
                {
                    rb.Background = new SolidColorBrush(Color.FromRgb(48, 48, 52));
                    rb.BorderBrush = new SolidColorBrush(Color.FromRgb(224, 64, 96));
                    rb.BorderThickness = new Thickness(1.5);
                    rb.Foreground = Brushes.White;
                }
                else
                {
                    rb.Background = new SolidColorBrush(Color.FromRgb(28, 28, 30));
                    rb.BorderBrush = new SolidColorBrush(Color.FromArgb(24, 255, 255, 255));
                    rb.BorderThickness = new Thickness(1);
                    rb.Foreground = new SolidColorBrush(Color.FromRgb(152, 152, 159));
                }
            }
        }

        private void BuildMemberCards()
        {
            PanelMembers.Children.Clear();
            var radioStyle = (Style)FindResource("FluentRadioCard");
            bool isLight = _selectedTheme.Equals("light", StringComparison.OrdinalIgnoreCase);

            foreach (var member in MemberInfo.Members)
            {
                var memberCol = member.GetThemeColor(isLight);
                var memberBrush = member.GetThemeBrush(isLight);

                var contentStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

                var dot = new System.Windows.Shapes.Ellipse
                {
                    Width = 9,
                    Height = 9,
                    Fill = memberBrush,
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                contentStack.Children.Add(dot);

                var txtMain = new TextBlock
                {
                    Text = member.NameKo,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = isLight ? new SolidColorBrush(Color.FromRgb(29, 29, 31)) : Brushes.White,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                };
                contentStack.Children.Add(txtMain);

                var txtSub = new TextBlock
                {
                    Text = $" ({member.NameEn}) · {member.ColorName}",
                    Foreground = isLight ? new SolidColorBrush(Color.FromRgb(120, 120, 128)) : new SolidColorBrush(Color.FromRgb(160, 160, 160)),
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center
                };
                contentStack.Children.Add(txtSub);

                bool isSelected = member.Id.Equals(_selectedMember, StringComparison.OrdinalIgnoreCase);

                var rb = new RadioButton
                {
                    Content = contentStack,
                    GroupName = "ResceneMembers",
                    Style = radioStyle,
                    Tag = member.Id,
                    IsChecked = isSelected
                };

                // 테마 및 선택 상태에 따른 라디오 배경 및 보더 적용 (선택 시 해당 멤버 상징색 하이라이트)
                if (isLight)
                {
                    if (isSelected)
                    {
                        rb.Background = new SolidColorBrush(Color.FromArgb(24, memberCol.R, memberCol.G, memberCol.B));
                        rb.BorderBrush = memberBrush;
                        rb.BorderThickness = new Thickness(1.5);
                    }
                    else
                    {
                        rb.Background = new SolidColorBrush(Color.FromRgb(242, 242, 247));
                        rb.BorderBrush = new SolidColorBrush(Color.FromRgb(218, 218, 222));
                        rb.BorderThickness = new Thickness(1);
                    }
                }
                else
                {
                    if (isSelected)
                    {
                        rb.Background = new SolidColorBrush(Color.FromArgb(45, memberCol.R, memberCol.G, memberCol.B));
                        rb.BorderBrush = memberBrush;
                        rb.BorderThickness = new Thickness(1.5);
                    }
                    else
                    {
                        rb.Background = new SolidColorBrush(Color.FromRgb(28, 28, 30));
                        rb.BorderBrush = new SolidColorBrush(Color.FromArgb(24, 255, 255, 255));
                        rb.BorderThickness = new Thickness(1);
                    }
                }

                rb.Checked += (s, e) =>
                {
                    if (rb.Tag is string id)
                    {
                        _selectedMember = id;
                        BuildMemberCards();
                        ApplyLivePreview();
                    }
                };

                PanelMembers.Children.Add(rb);
            }
        }

        private void RadioTheme_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            _selectedTheme = RadioThemeLight.IsChecked == true ? "light" : "dark";
            ApplyThemeToWindow(_selectedTheme.Equals("light", StringComparison.OrdinalIgnoreCase));
            UpdateSegmentRadioVisuals();
            BuildMemberCards();
            ApplyLivePreview();
        }

        private void RadioLang_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            _selectedLanguage = RadioLangEn.IsChecked == true ? "en" : "ko";
            LocalizationManager.CurrentLanguage = _selectedLanguage;
            ApplyLocalizationTexts();
            UpdateSegmentRadioVisuals();
            BuildMemberCards();
            ApplyLivePreview();
        }

        private void RadioStyle_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            if (RadioStyleCompact.IsChecked == true) _selectedTimerStyle = "compact";
            else if (RadioStyleStandard.IsChecked == true) _selectedTimerStyle = "standard";
            else if (RadioStyleRing.IsChecked == true) _selectedTimerStyle = "ring";

            UpdateSegmentRadioVisuals();
            UpdateTimerStyleDetailText();
            ApplyLivePreview();
        }

        private void ApplyLivePreview()
        {
            if (_isInitializing) return;

            _settings.Theme = _selectedTheme;
            _settings.Language = _selectedLanguage;
            _settings.TimerStyle = _selectedTimerStyle;
            _settings.Member = _selectedMember;
            _settings.CharacterSize = SliderSize.Value;
            _settings.CharacterOpacity = SliderOpacity.Value;
            _settings.AlwaysOnTop = ChkAlwaysOnTop.IsChecked ?? true;
            _settings.AlwaysShowTime = ChkAlwaysShowTime.IsChecked ?? false;
            _settings.PlaySound = ChkPlaySound.IsChecked ?? true;

            if (int.TryParse(TxtFocusInput.Text, out int fVal))
                _settings.FocusMinutes = Math.Clamp(fVal, (int)SliderFocus.Minimum, (int)SliderFocus.Maximum);
            else
                _settings.FocusMinutes = (int)SliderFocus.Value;

            if (int.TryParse(TxtBreakInput.Text, out int bVal))
                _settings.BreakMinutes = Math.Clamp(bVal, (int)SliderBreak.Minimum, (int)SliderBreak.Maximum);
            else
                _settings.BreakMinutes = (int)SliderBreak.Value;

            if (int.TryParse(TxtLongBreakInput.Text, out int lbVal))
                _settings.LongBreakMinutes = Math.Clamp(lbVal, (int)SliderLongBreak.Minimum, (int)SliderLongBreak.Maximum);
            else
                _settings.LongBreakMinutes = (int)SliderLongBreak.Value;

            _mainWindow.ApplySettings();
        }

        private void UpdateTimerStyleDetailText()
        {
            LblTimerStyleDetail.Text = _selectedTimerStyle switch
            {
                "standard" => LocalizationManager.Get("TimerStyleStandardDesc"),
                "ring" => LocalizationManager.Get("TimerStyleRingDesc"),
                _ => LocalizationManager.Get("TimerStyleCompactDesc")
            };
        }

        #region Slider Events

        private void SliderFocus_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtFocusInput != null)
                TxtFocusInput.Text = ((int)e.NewValue).ToString();
            ApplyLivePreview();
        }

        private void SliderBreak_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtBreakInput != null)
                TxtBreakInput.Text = ((int)e.NewValue).ToString();
            ApplyLivePreview();
        }

        private void SliderLongBreak_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtLongBreakInput != null)
                TxtLongBreakInput.Text = ((int)e.NewValue).ToString();
            ApplyLivePreview();
        }

        private void SliderSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtSizeVal != null)
                TxtSizeVal.Text = $"{(int)e.NewValue}px";
            ApplyLivePreview();
        }

        private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtOpacityVal != null)
                TxtOpacityVal.Text = $"{(int)(e.NewValue * 100)}%";
            ApplyLivePreview();
        }

        private void ChkOption_Click(object sender, RoutedEventArgs e)
        {
            ApplyLivePreview();
        }

        #endregion

        #region NumberBox Keyboard Inputs

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as TextBox)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void TxtFocusInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtFocusInput.Text, out int val))
            {
                val = Math.Clamp(val, (int)SliderFocus.Minimum, (int)SliderFocus.Maximum);
                SliderFocus.Value = val;
                TxtFocusInput.Text = val.ToString();
            }
            else
            {
                TxtFocusInput.Text = ((int)SliderFocus.Value).ToString();
            }
            ApplyLivePreview();
        }

        private void TxtBreakInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtBreakInput.Text, out int val))
            {
                val = Math.Clamp(val, (int)SliderBreak.Minimum, (int)SliderBreak.Maximum);
                SliderBreak.Value = val;
                TxtBreakInput.Text = val.ToString();
            }
            else
            {
                TxtBreakInput.Text = ((int)SliderBreak.Value).ToString();
            }
            ApplyLivePreview();
        }

        private void TxtLongBreakInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtLongBreakInput.Text, out int val))
            {
                val = Math.Clamp(val, (int)SliderLongBreak.Minimum, (int)SliderLongBreak.Maximum);
                SliderLongBreak.Value = val;
                TxtLongBreakInput.Text = val.ToString();
            }
            else
            {
                TxtLongBreakInput.Text = ((int)SliderLongBreak.Value).ToString();
            }
            ApplyLivePreview();
        }

        #endregion

        #region CommandBar Buttons & Closing

        private void BtnOpenImages_Click(object sender, RoutedEventArgs e)
        {
            ImageHelper.OpenImagesFolder();
        }

        private void BtnOpenSounds_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.OpenSoundsFolder();
        }

        private bool HasUnsavedChanges()
        {
            if (_hasSaved) return false;

            int focus = int.TryParse(TxtFocusInput.Text, out int f) ? f : (int)SliderFocus.Value;
            int brk = int.TryParse(TxtBreakInput.Text, out int b) ? b : (int)SliderBreak.Value;
            int lbrk = int.TryParse(TxtLongBreakInput.Text, out int lb) ? lb : (int)SliderLongBreak.Value;

            return !_selectedTheme.Equals(_origTheme, StringComparison.OrdinalIgnoreCase)
                || !_selectedLanguage.Equals(_origLanguage, StringComparison.OrdinalIgnoreCase)
                || !_selectedTimerStyle.Equals(_origTimerStyle, StringComparison.OrdinalIgnoreCase)
                || !_selectedMember.Equals(_origMember, StringComparison.OrdinalIgnoreCase)
                || Math.Abs(SliderSize.Value - _origSize) > 0.5
                || Math.Abs(SliderOpacity.Value - _origOpacity) > 0.01
                || (ChkAlwaysOnTop.IsChecked ?? true) != _origAlwaysOnTop
                || (ChkAlwaysShowTime.IsChecked ?? false) != _origAlwaysShowTime
                || (ChkPlaySound.IsChecked ?? true) != _origPlaySound
                || focus != _origFocus
                || brk != _origBreak
                || lbrk != _origLongBreak;
        }

        private void RevertSettings()
        {
            _settings.Theme = _origTheme;
            _settings.Language = _origLanguage;
            _settings.TimerStyle = _origTimerStyle;
            _settings.Member = _origMember;
            _settings.CharacterSize = _origSize;
            _settings.CharacterOpacity = _origOpacity;
            _settings.AlwaysOnTop = _origAlwaysOnTop;
            _settings.AlwaysShowTime = _origAlwaysShowTime;
            _settings.PlaySound = _origPlaySound;
            _settings.FocusMinutes = _origFocus;
            _settings.BreakMinutes = _origBreak;
            _settings.LongBreakMinutes = _origLongBreak;

            _mainWindow.ApplySettings();
        }

        private void SaveSettings()
        {
            if (int.TryParse(TxtFocusInput.Text, out int fVal))
                SliderFocus.Value = Math.Clamp(fVal, (int)SliderFocus.Minimum, (int)SliderFocus.Maximum);

            if (int.TryParse(TxtBreakInput.Text, out int bVal))
                SliderBreak.Value = Math.Clamp(bVal, (int)SliderBreak.Minimum, (int)SliderBreak.Maximum);

            if (int.TryParse(TxtLongBreakInput.Text, out int lbVal))
                SliderLongBreak.Value = Math.Clamp(lbVal, (int)SliderLongBreak.Minimum, (int)SliderLongBreak.Maximum);

            _settings.Theme = _selectedTheme;
            _settings.Language = _selectedLanguage;
            _settings.TimerStyle = _selectedTimerStyle;
            _settings.Member = _selectedMember;
            _settings.FocusMinutes = (int)SliderFocus.Value;
            _settings.BreakMinutes = (int)SliderBreak.Value;
            _settings.LongBreakMinutes = (int)SliderLongBreak.Value;
            _settings.CharacterSize = SliderSize.Value;
            _settings.CharacterOpacity = SliderOpacity.Value;
            _settings.AlwaysOnTop = ChkAlwaysOnTop.IsChecked ?? true;
            _settings.AlwaysShowTime = ChkAlwaysShowTime.IsChecked ?? false;
            _settings.PlaySound = ChkPlaySound.IsChecked ?? true;

            _settings.Save();
            _mainWindow.ApplySettings();
            _hasSaved = true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_hasSaved) return;

            if (HasUnsavedChanges())
            {
                string title = LocalizationManager.IsEnglish ? "Settings" : "설정";
                string msg = LocalizationManager.IsEnglish 
                    ? "Do you want to save your changes?" 
                    : "변경 사항을 저장하겠습니까?";

                var result = MessageBox.Show(this, msg, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveSettings();
                }
                else if (result == MessageBoxResult.No)
                {
                    RevertSettings();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion
    }
}
