using System.Drawing.Drawing2D;

namespace DNSphere;

/// <summary>
/// دکمه‌ی گرد با آیکون فونت سیستم.
/// Primary = دکمه‌ی توپر، در غیر این صورت خطی. رنگ و گردی گوشه‌ها قابل تنظیم است.
/// </summary>
public class IconButton : Control
{
    private readonly Font _fText = UiFont.Get(10F, FontStyle.Bold);
    private readonly Font _fIcon = Icons.GetFont(13F);
    private Color _accent = Color.FromArgb(63, 81, 181);
    private bool _hover;
    private bool _down;

    public string Glyph { get; set; } = "";
    public bool Primary { get; set; }
    public int Radius { get; set; } = 8;

    public Color AccentColor
    {
        get => _accent;
        set { _accent = value; Invalidate(); }
    }

    public IconButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.Selectable, true);
        Cursor = Cursors.Hand;
        Size = new Size(200, 40);
    }

    protected override void OnParentBackColorChanged(EventArgs e) { Invalidate(); base.OnParentBackColorChanged(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (Enabled && (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter))
        {
            OnClick(EventArgs.Empty);
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    private static Color Mix(Color a, Color b, float t) => Color.FromArgb(
        (int)(a.R + (b.R - a.R) * t),
        (int)(a.G + (b.G - a.G) * t),
        (int)(a.B + (b.B - a.B) * t));

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? SystemColors.Control);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = RoundRect(rect, Radius);

        Color fill, border, fore;
        if (!Enabled)
        {
            fill = Color.FromArgb(232, 236, 244);
            border = fill;
            fore = Color.FromArgb(148, 160, 180);
        }
        else if (Primary)
        {
            fill = _down ? Mix(_accent, Color.Black, 0.30F)
                 : _hover ? Mix(_accent, Color.Black, 0.12F)
                 : _accent;
            border = fill;
            fore = Color.White;
        }
        else
        {
            fill = _down ? Mix(Color.White, _accent, 0.22F)
                 : _hover ? Mix(Color.White, _accent, 0.10F)
                 : Color.White;
            border = _accent;
            fore = _accent;
        }

        using (var b = new SolidBrush(fill)) g.FillPath(b, path);
        using (var p = new Pen(border, Focused && Enabled ? 2F : 1.2F)) g.DrawPath(p, path);

        // متن راست‌چین (RTL) و آیکون سمت راستِ متن؛ کل مجموعه وسط دکمه قرار می‌گیرد
        Size ts = TextRenderer.MeasureText(Text, _fText, new Size(int.MaxValue, int.MaxValue),
            TextFormatFlags.NoPadding | TextFormatFlags.RightToLeft);
        bool hasIcon = !string.IsNullOrEmpty(Glyph);
        int iconW = hasIcon ? 22 : 0;
        int gap = hasIcon ? 8 : 0;
        int textW = Math.Min(ts.Width, Math.Max(10, Width - 16 - iconW - gap));
        int total = textW + gap + iconW;
        int x = Math.Max(8, (Width - total) / 2);

        TextRenderer.DrawText(g, Text, _fText, new Rectangle(x, 0, textW, Height), fore,
            TextFormatFlags.Right | TextFormatFlags.RightToLeft | TextFormatFlags.VerticalCenter |
            TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

        if (hasIcon)
            TextRenderer.DrawText(g, Glyph, _fIcon, new Rectangle(x + textW + gap, 0, iconW, Height), fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    private static GraphicsPath RoundRect(Rectangle r, int radius)
    {
        radius = Math.Max(1, Math.Min(radius, Math.Min(r.Width, r.Height) / 2));
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
            _fText.Dispose();
            _fIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
