using System.Collections.Generic;
using System.Windows.Media;

namespace radiant_noether
{
    public class MemberInfo
    {
        public string Id { get; set; } = string.Empty;
        public string NameKo { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public Color ThemeColor { get; set; }

        public static readonly List<MemberInfo> Members = new()
        {
            new MemberInfo { Id = "woni", NameKo = "원이", NameEn = "Woni", ThemeColor = Color.FromRgb(255, 138, 168) },
            new MemberInfo { Id = "liv", NameKo = "리브", NameEn = "Liv", ThemeColor = Color.FromRgb(178, 143, 237) },
            new MemberInfo { Id = "minami", NameKo = "미나미", NameEn = "Minami", ThemeColor = Color.FromRgb(126, 196, 247) },
            new MemberInfo { Id = "may", NameKo = "메이", NameEn = "May", ThemeColor = Color.FromRgb(120, 220, 185) },
            new MemberInfo { Id = "zena", NameKo = "제나", NameEn = "Zena", ThemeColor = Color.FromRgb(255, 205, 105) },
        };

        public static MemberInfo GetById(string id)
        {
            var found = Members.Find(m => m.Id.Equals(id, System.StringComparison.OrdinalIgnoreCase));
            return found ?? Members[0];
        }
    }
}
