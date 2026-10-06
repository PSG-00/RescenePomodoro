using System.Collections.Generic;
using System.Windows.Media;

namespace radiant_noether
{
    public class MemberInfo
    {
        public string Id { get; set; } = string.Empty;
        public string NameKo { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string HexCode { get; set; } = string.Empty;
        public Color BaseColor { get; set; }
        public Color LightModeColor { get; set; }
        public Color DarkModeColor { get; set; }

        public Color ThemeColor => BaseColor;

        public Color GetThemeColor(bool isLight) => isLight ? LightModeColor : DarkModeColor;

        public Brush GetThemeBrush(bool isLight)
        {
            var brush = new SolidColorBrush(GetThemeColor(isLight));
            brush.Freeze();
            return brush;
        }

        public static readonly List<MemberInfo> Members = new()
        {
            new MemberInfo
            {
                Id = "woni",
                NameKo = "원이",
                NameEn = "Woni",
                ColorName = "Watercourse",
                HexCode = "#045a42",
                BaseColor = Color.FromRgb(4, 90, 66),
                LightModeColor = Color.FromRgb(4, 90, 66),      // 깊고 차분한 포레스트 에메랄드
                DarkModeColor = Color.FromRgb(16, 185, 129)     // 다크모드에서 빛나는 선명한 에메랄드 그린
            },
            new MemberInfo
            {
                Id = "liv",
                NameKo = "리브",
                NameEn = "Liv",
                ColorName = "Black",
                HexCode = "#000000",
                BaseColor = Color.FromRgb(0, 0, 0),
                LightModeColor = Color.FromRgb(24, 24, 27),     // 시크하고 정제된 피치 챠콜 블랙
                DarkModeColor = Color.FromRgb(226, 232, 240)    // 다크모드 배경에 묻히지 않는 고급 플래티넘 실버
            },
            new MemberInfo
            {
                Id = "minami",
                NameKo = "미나미",
                NameEn = "Minami",
                ColorName = "Pelorous",
                HexCode = "#2b99c4",
                BaseColor = Color.FromRgb(43, 153, 196),
                LightModeColor = Color.FromRgb(35, 130, 168),   // 화이트 배경에서 또렷한 딥 펠로러스
                DarkModeColor = Color.FromRgb(43, 153, 196)     // 청량하고 선명한 펠로러스 오션 블루
            },
            new MemberInfo
            {
                Id = "may",
                NameKo = "메이",
                NameEn = "May",
                ColorName = "Portica",
                HexCode = "#ecd25b",
                BaseColor = Color.FromRgb(236, 210, 91),
                LightModeColor = Color.FromRgb(202, 142, 16),   // 화이트 배경에서도 가독성 우수한 웜 골든 앰버
                DarkModeColor = Color.FromRgb(236, 210, 91)     // 화사하게 빛나는 오리지널 포르티카 옐로우
            },
            new MemberInfo
            {
                Id = "zena",
                NameKo = "제나",
                NameEn = "Zena",
                ColorName = "Biloba Flower",
                HexCode = "#ba92db",
                BaseColor = Color.FromRgb(186, 146, 219),
                LightModeColor = Color.FromRgb(145, 100, 185),  // 화이트 배경에서 품격 있는 빌로바 바이올렛
                DarkModeColor = Color.FromRgb(186, 146, 219)    // 신비롭고 몽환적인 빌로바 플라워 라벤더
            },
        };

        public static MemberInfo GetById(string id)
        {
            var found = Members.Find(m => m.Id.Equals(id, System.StringComparison.OrdinalIgnoreCase));
            return found ?? Members[0];
        }
    }
}
