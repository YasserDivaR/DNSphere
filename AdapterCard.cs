using System.Drawing.Drawing2D;

namespace DNSphere;

/// <summary>کارت گرافیکی نمایش‌دهنده‌ی یک کارت شبکه</summary>
public class AdapterCard : Control
{
    private bool _selected;
    private bool _hover;

    private readonly Font _fName = UiFont.Get(11F, FontStyle.Bold);
    private readonly Font _fSmall = UiFont.Get(9F);
    private readonly Font _fStatus = UiFont.Get(9.5F, FontStyle.Bold);
    private readonly Font _fIcon = Icons.GetFont(16F);

    public AdapterInfo Adapter { get; }

    public bool Selected
    {
        get => _selected;
        set { _selected = value; Invalidate(); }
    }

    public AdapterCard(AdapterInfo adapter)
    {
        Adapter = adapter;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(800, 76);
        Margin = new Padding(0, 0, 0, 8);
        Cursor = Cursors.Hand;
    }

    protected override void OnParentBackColorChanged(EventArgs e) { Invalidate(); base.OnParentBackColorChanged(e); }
    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? SystemColors.Control);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = RoundRect(rect, 12);

        Color fill = _selected ? Color.FromArgb(232, 240, 254)
                   : _hover ? Color.FromArgb(244, 247, 253)
                   : Color.White;
        Color border = _selected ? Color.FromArgb(37, 99, 235) : Color.FromArgb(209, 216, 230);

        using (var b = new SolidBrush(fill)) g.FillPath(b, path);
        using (var p = new Pen(border, _selected ? 2.2F : 1F)) g.DrawPath(p, path);

        // وضعیت
        Color dot;
        string status;
        if (Adapter.IsConnected) { dot = Color.FromArgb(22, 163, 74); status = "فعال و متصل"; }
        else if (Adapter.IsUp) { dot = Color.FromArgb(245, 158, 11); status = "فعال (بدون گیتوی)"; }
        else { dot = Color.FromArgb(148, 163, 184); status = "غیرفعال"; }

        var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;

        // متن وضعیت در سمت راست
        Size st = TextRenderer.MeasureText(status, _fStatus, new Size(int.MaxValue, int.MaxValue),
            TextFormatFlags.NoPadding);
        int statusX = Width - 18 - st.Width;
        TextRenderer.DrawText(g, status, _fStatus, new Rectangle(statusX, 8, st.Width + 4, 22), dot,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        using (var db = new SolidBrush(dot))
            g.FillEllipse(db, statusX - 16, 13, 10, 10);

        // آیکون کارت (Ethernet / Wi-Fi) داخل دایره‌ی رنگی
        var circle = new Rectangle(14, 17, 42, 42);
        using (var cb = new SolidBrush(Color.FromArgb(38, dot)))
            g.FillEllipse(cb, circle);
        TextRenderer.DrawText(g, Adapter.IsWireless ? Icons.Wifi : Icons.Ethernet, _fIcon, circle, dot,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        const int textX = 70;
        int textWidth = Math.Max(50, statusX - textX - 26);

        // خط اول: نام کارت
        TextRenderer.DrawText(g, Adapter.Name, _fName, new Rectangle(textX, 6, textWidth, 22),
            Color.FromArgb(30, 41, 59), flags);

        // خط دوم: توضیحات کارت
        TextRenderer.DrawText(g, Adapter.Description, _fSmall, new Rectangle(textX, 29, Width - textX - 16, 20),
            Color.FromArgb(100, 116, 139), flags);

        // خط سوم: IP / DNS تنظیم‌شده / MAC
        string details = $"IPv4: {Adapter.Ipv4}    DNS: {Adapter.Dns}    MAC: {Adapter.Mac}";
        TextRenderer.DrawText(g, details, _fSmall, new Rectangle(textX, 50, Width - textX - 16, 20),
            Color.FromArgb(71, 85, 105), flags);
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fName.Dispose();
            _fSmall.Dispose();
            _fStatus.Dispose();
            _fIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
