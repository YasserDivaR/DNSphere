using System.Drawing.Drawing2D;

namespace DNSphere;

/// <summary>پس‌زمینه‌ی مدرن برنامه: گرادیان ملایم (بدون شفافیت روی کنترل‌های سنگین)</summary>
public class BodyPanel : Panel
{
    public BodyPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.FromArgb(236, 241, 252);
    }

    private static readonly Color Top = Color.FromArgb(244, 247, 254);
    private static readonly Color Bottom = Color.FromArgb(224, 233, 250);

    /// <summary>رنگ گرادیان در ارتفاع y (برای هم‌رنگ کردن برچسب‌ها با پس‌زمینه)</summary>
    public Color ColorAtY(int y)
    {
        float t = Height <= 0 ? 0F : Math.Max(0F, Math.Min(1F, y / (float)Height));
        return Color.FromArgb(
            (int)(Top.R + (Bottom.R - Top.R) * t),
            (int)(Top.G + (Bottom.G - Top.G) * t),
            (int)(Top.B + (Bottom.B - Top.B) * t));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        using var br = new LinearGradientBrush(ClientRectangle, Top, Bottom, 90F);
        e.Graphics.FillRectangle(br, ClientRectangle);
    }
}

/// <summary>
/// جعبه‌ی گرد با حاشیه‌ی ظریف و نوار رنگی کنار.
/// هم برای جعبه‌ی دکمه‌ها و هم قاب لیست کارت‌های شبکه و DNS استفاده می‌شود.
/// رنگ زمینه همان BackColor است و باید با BackColor کنترل داخلش یکی باشد.
/// </summary>
/// <summary>پنل تخت با رنگ ثابت (تم Material رنگ آن را عوض نمی‌کند)</summary>
public class SolidPanel : Panel
{
    private Color _fill = Color.FromArgb(224, 233, 250);

    public Color FillColor
    {
        get => _fill;
        set { _fill = value; Invalidate(); }
    }

    public override Color BackColor
    {
        get => _fill;
        set { Invalidate(); }
    }

    public SolidPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e) => e.Graphics.Clear(_fill);
}

public class BoxPanel : Panel
{
    private Color _fill = Color.White;

    public int Radius { get; set; } = 16;
    public Color BorderColor { get; set; } = Color.FromArgb(207, 216, 234);

    /// <summary>رنگ زمینه‌ی جعبه. از BackColor جدا شده چون تم Material BackColor را خاکستری می‌کند.</summary>
    public Color FillColor
    {
        get => _fill;
        set { _fill = value; Invalidate(); }
    }

    /// <summary>تم Material نمی‌تواند رنگ زمینه را عوض کند؛ همیشه FillColor برمی‌گردد.</summary>
    public override Color BackColor
    {
        get => _fill;
        set { Invalidate(); }
    }

    /// <summary>اگر مقدار داشته باشد، یک نوار رنگی باریک لبه‌ی چپ قاب کشیده می‌شود</summary>
    public Color StripeColor { get; set; } = Color.Empty;

    public BoxPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (Width <= 0 || Height <= 0) return;
        using var path = RoundRect(new Rectangle(0, 0, Width, Height), Radius);
        var old = Region;
        Region = new Region(path);
        old?.Dispose();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        if (StripeColor != Color.Empty)
        {
            using var sb = new SolidBrush(StripeColor);
            g.FillRectangle(sb, 0, 0, 6, Height);
        }

        using var path = RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), Radius);
        using var pen = new Pen(BorderColor, 1.4F);
        g.DrawPath(pen, path);
    }

    private static GraphicsPath RoundRect(Rectangle r, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

/// <summary>
/// FlowLayoutPanel با رنگ زمینه‌ی مقاوم در برابر تم Material، بافر دوبل
/// و بدون نوار اسکرول خاکستری (نوار سفارشی ModernScrollBar رویش قرار می‌گیرد).
/// </summary>
public class ThemeFlowPanel : FlowLayoutPanel
{
    private Color _fill = Color.White;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
    private const int SB_HORZ = 0;
    private const int WM_NCCALCSIZE = 0x0083;

    /// <summary>اسکرول افقی هرگز لازم نیست؛ با هر محاسبه‌ی ناحیه‌ی کلاینت مخفی می‌شود</summary>
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_NCCALCSIZE && IsHandleCreated) ShowScrollBar(Handle, SB_HORZ, false);
        base.WndProc(ref m);
    }

    public ThemeFlowPanel()
    {
        DoubleBuffered = true;
    }

    public Color FillColor
    {
        get => _fill;
        set { _fill = value; Invalidate(); }
    }

    public override Color BackColor
    {
        get => _fill;
        set { Invalidate(); }
    }
}

/// <summary>جدول چیدمان با رنگ زمینه‌ی ثابت (تم Material نمی‌تواند آن را عوض کند)</summary>
public class ThemeTablePanel : TableLayoutPanel
{
    private Color _fill = Color.White;

    public ThemeTablePanel() { DoubleBuffered = true; }

    public Color FillColor
    {
        get => _fill;
        set { _fill = value; Invalidate(); }
    }

    public override Color BackColor
    {
        get => _fill;
        set { Invalidate(); }
    }
}

/// <summary>
/// تم Material رنگ پس‌زمینه و متن برچسب‌ها را عوض می‌کند. این کلاس رنگ دلخواه را ثابت نگه می‌دارد.
/// خروجی Pin یک Action است که هر زمان لازم بود (تغییر اندازه، Shown) می‌شود دوباره صدایش زد.
/// </summary>
public static class ThemeGuard
{
    public static Action Pin(Label l, Func<Color> back, Color fore)
    {
        void Fix()
        {
            var b = back();
            if (l.BackColor != b) l.BackColor = b;
            if (l.ForeColor != fore) l.ForeColor = fore;
        }
        l.BackColorChanged += (s, e) => Fix();
        l.ForeColorChanged += (s, e) => Fix();
        Fix();
        return Fix;
    }
}

public enum StatusKind { Neutral, Ok, Error }

/// <summary>
/// رندر مدرن StatusStrip: زمینه هم‌رنگ پایین بدنه و یک «قرص» گرد هم‌عرض باکس‌ها که
/// رنگش با نوع پیام (عادی / موفق / خطا) عوض می‌شود. مستقل از رنگ‌های تم Material.
/// </summary>
public class StatusRenderer : ToolStripProfessionalRenderer
{
    private readonly Color _back;

    public StatusKind Kind { get; set; } = StatusKind.Neutral;

    public StatusRenderer(Color back)
    {
        _back = back;
        RoundedEdges = false;
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        var g = e.Graphics;
        using (var b = new SolidBrush(_back))
            g.FillRectangle(b, e.AffectedBounds);

        Color fill, border;
        switch (Kind)
        {
            case StatusKind.Ok:
                fill = Color.FromArgb(238, 250, 242); border = Color.FromArgb(178, 226, 197); break;
            case StatusKind.Error:
                fill = Color.FromArgb(254, 240, 240); border = Color.FromArgb(246, 190, 190); break;
            default:
                fill = Color.FromArgb(250, 251, 255); border = Color.FromArgb(198, 207, 238); break;
        }

        // هم‌تراز با باکس‌های بالا (از x=16 تا ۱۲ پیکسل مانده به لبه‌ی راست)
        var r = new Rectangle(16, 4, e.ToolStrip.Width - 16 - 12 - 1, e.ToolStrip.Height - 9);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = RoundRect(r, 12);
        using (var fb = new SolidBrush(fill)) g.FillPath(fb, path);
        using (var pen = new Pen(border, 1.4F)) g.DrawPath(pen, path);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        // بدون حاشیه‌ی پیش‌فرض
    }

    private static GraphicsPath RoundRect(Rectangle r, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
