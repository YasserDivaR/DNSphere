using System.Drawing.Drawing2D;

namespace DNSphere;

/// <summary>کارت گرافیکی یک DNS عمومی (پالت سبز-آبی) همراه با نمایش پینگ</summary>
public class DnsCard : Control
{
    private static readonly Color Accent = Color.FromArgb(13, 148, 136);
    private static readonly Color FillNormal = Color.FromArgb(247, 253, 252);
    private static readonly Color FillHover = Color.FromArgb(233, 248, 245);
    private static readonly Color FillSelected = Color.FromArgb(204, 240, 234);
    private static readonly Color BorderNormal = Color.FromArgb(178, 224, 218);
    private static readonly Color TextMain = Color.FromArgb(19, 78, 74);
    private static readonly Color TextSub = Color.FromArgb(55, 112, 108);

    private enum PingState { Idle, Measuring, Ok, Failed }

    private bool _selected;
    private bool _hover;
    private PingState _state = PingState.Idle;

    private readonly Font _fName = UiFont.Get(10F, FontStyle.Bold);
    private readonly Font _fAddr = UiFont.Get(9F);
    private readonly Font _fPing = UiFont.Get(9F, FontStyle.Bold);
    private readonly Font _fIcon = Icons.GetFont(14F);

    public DnsEntry Entry { get; }

    /// <summary>پینگ بر حسب میلی‌ثانیه؛ منفی یعنی اندازه‌گیری نشده یا بدون پاسخ</summary>
    public long PingMs { get; private set; } = -1;

    public bool Selected
    {
        get => _selected;
        set { _selected = value; Invalidate(); }
    }

    public DnsCard(DnsEntry entry)
    {
        Entry = entry;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(290, 62);
        Margin = new Padding(0, 0, 10, 10);
        Cursor = Cursors.Hand;
    }

    public void SetPingMeasuring()
    {
        _state = PingState.Measuring;
        PingMs = -1;
        Invalidate();
    }

    public void SetPing(long ms)
    {
        PingMs = ms;
        _state = ms >= 0 ? PingState.Ok : PingState.Failed;
        Invalidate();
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
        using var path = RoundRect(rect, 10);

        Color fill = _selected ? FillSelected : _hover ? FillHover : FillNormal;
        Color border = _selected ? Accent : BorderNormal;

        using (var b = new SolidBrush(fill)) g.FillPath(b, path);
        using (var p = new Pen(border, _selected ? 2F : 1F)) g.DrawPath(p, path);

        // آیکون داخل دایره
        var circle = new Rectangle(12, 13, 36, 36);
        using (var cb = new SolidBrush(Color.FromArgb(_selected ? 70 : 38, Accent)))
            g.FillEllipse(cb, circle);
        TextRenderer.DrawText(g, _selected ? Icons.Check : Entry.IsCustom ? Icons.Person : Icons.Globe, _fIcon, circle, Accent,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
        const int x = 58;
        int w = Math.Max(30, Width - x - 10);

        // نشان پینگ (بالا، سمت راست)
        int nameW = w;
        if (_state != PingState.Idle)
        {
            string pt;
            Color pc;
            switch (_state)
            {
                case PingState.Measuring: pt = "..."; pc = Color.FromArgb(100, 116, 139); break;
                case PingState.Failed: pt = "timeout"; pc = Color.FromArgb(220, 38, 38); break;
                default:
                    pt = PingMs + " ms";
                    pc = PingMs < 80 ? Color.FromArgb(22, 163, 74)
                       : PingMs < 200 ? Color.FromArgb(217, 119, 6)
                       : Color.FromArgb(220, 38, 38);
                    break;
            }

            Size ps = TextRenderer.MeasureText(pt, _fPing, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding);
            int px = Width - 12 - ps.Width;
            using (var db = new SolidBrush(pc))
                g.FillEllipse(db, px - 13, 16, 8, 8);
            TextRenderer.DrawText(g, pt, _fPing, new Rectangle(px, 9, ps.Width + 4, 22), pc,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            nameW = Math.Max(30, px - 18 - x);
        }

        TextRenderer.DrawText(g, Entry.Name, _fName, new Rectangle(x, 9, nameW, 22), TextMain, flags);

        string addr = string.IsNullOrEmpty(Entry.Secondary)
            ? Entry.Primary
            : $"{Entry.Primary} · {Entry.Secondary}";
        TextRenderer.DrawText(g, addr, _fAddr, new Rectangle(x, 32, w, 20), TextSub, flags);
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
            _fAddr.Dispose();
            _fPing.Dispose();
            _fIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
