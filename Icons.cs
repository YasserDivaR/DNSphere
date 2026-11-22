using System.Drawing.Text;

namespace DNSphere;

/// <summary>
/// آیکون‌ها از فونت آیکون سیستم: Segoe Fluent Icons (ویندوز ۱۱)
/// و در غیر این صورت Segoe MDL2 Assets (ویندوز ۱۰).
/// </summary>
public static class Icons
{
    // کدهای گلیف (در هر دو فونت یکسان‌اند)
    public const string Ethernet = "";
    public const string Wifi = "";
    public const string Globe = "";
    public const string Refresh = "";
    public const string Check = "";
    public const string Settings = "";
    public const string Restore = "";
    public const string Speed = "";
    public const string Warning = "\uE7BA";
    public const string Info = "\uE946";
    public const string Person = "\uE77B";
    public const string Add = "\uE710";
    public const string Edit = "\uE70F";
    public const string Delete = "\uE74D";
    public const string Upload = "\uE898";
    public const string Flush = "\uE74D";
    public const string Download = "";

    private static string _family;

    public static string Family
    {
        get
        {
            if (_family != null) return _family;
            using var fonts = new InstalledFontCollection();
            var names = fonts.Families.Select(f => f.Name).ToList();
            _family = names.Contains("Segoe Fluent Icons") ? "Segoe Fluent Icons" : "Segoe MDL2 Assets";
            return _family;
        }
    }

    /// <summary>فونت آیکون با اندازه به پوینت</summary>
    public static Font GetFont(float points) => new Font(Family, points, FontStyle.Regular);

    /// <summary>یک گلیف را به صورت تصویر شفاف (برای Icon دکمه‌ها و ...) رسم می‌کند.</summary>
    public static Bitmap Render(string glyph, int size, Color color)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        using var font = new Font(Family, size * 0.72F, FontStyle.Regular, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(color);
        using var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString(glyph, font, brush, new RectangleF(0, 0, size, size), sf);
        return bmp;
    }

    /// <summary>آیکون پنجره‌ی برنامه</summary>
    public static Icon AppIcon(Color color)
    {
        using var bmp = Render(Globe, 32, color);
        using var tmp = Icon.FromHandle(bmp.GetHicon());
        return (Icon)tmp.Clone();
    }
}
