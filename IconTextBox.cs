using System.Drawing.Drawing2D;

namespace DNSphere;

/// <summary>
/// تکست‌باکس گرد با آیکون فونت سیستم و برچسب (Hint) در بالا.
/// فقط با کنترل‌های استاندارد WinForms ساخته شده و به هیچ کتابخانه‌ای وابسته نیست.
/// </summary>
public class IconTextBox : Control
{
    private readonly TextBox _tb = new() { BorderStyle = BorderStyle.None };
    private readonly Font _fIcon = Icons.GetFont(14F);
    private readonly Font _fHint = UiFont.Get(8.25F);
    private bool _focused;

    public string Hint { get; set; } = "";
    public string Glyph { get; set; } = Icons.Globe;

    public IconTextBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(260, 48);

        _tb.Font = UiFont.Get(11F);
        // راست‌چین؛ اعداد و نقطه‌ی IP همچنان به‌صورت چپ‌به‌راست خوانده می‌شوند
        _tb.RightToLeft = RightToLeft.Yes;
        _tb.TextAlign = HorizontalAlignment.Right;
        _tb.BackColor = Color.White;
        _tb.ForeColor = Color.FromArgb(30, 41, 59);
        // تم Material ممکن است رنگ پس‌زمینه را عوض کند؛ همیشه سفید نگهش می‌داریم
        _tb.BackColorChanged += (s, e) =>
        {
            if (_tb.BackColor != Color.White) _tb.BackColor = Color.White;
        };
        _tb.ForeColorChanged += (s, e) =>
        {
            if (_tb.ForeColor != Color.FromArgb(30, 41, 59)) _tb.ForeColor = Color.FromArgb(30, 41, 59);
        };
        _tb.TextChanged += (s, e) => OnTextChanged(EventArgs.Empty);
        _tb.GotFocus += (s, e) => { _focused = true; Invalidate(); };
        _tb.LostFocus += (s, e) => { _focused = false; Invalidate(); };
        Controls.Add(_tb);
        LayoutInner();
    }

    public override string Text
    {
        get => _tb.Text;
        set => _tb.Text = value;
    }

    private void LayoutInner()
    {
        // آیکون سمت راست است؛ متن از چپ تا کنار آیکون
        _tb.Location = new Point(14, 24);
        _tb.Width = Math.Max(20, Width - 14 - 50);
    }

    protected override void OnParentBackColorChanged(EventArgs e) { Invalidate(); base.OnParentBackColorChanged(e); }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutInner();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        _tb.Focus();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _tb.Focus();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? SystemColors.Control);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = RoundRect(rect, 10);
        using (var b = new SolidBrush(Color.White)) g.FillPath(b, path);

        Color border = _focused ? Color.FromArgb(37, 99, 235) : Color.FromArgb(200, 205, 215);
        using (var p = new Pen(border, _focused ? 2F : 1F)) g.DrawPath(p, path);

        // آیکون
        TextRenderer.DrawText(g, Glyph, _fIcon, new Rectangle(Width - 44, 0, 34, Height),
            _focused ? Color.FromArgb(37, 99, 235) : Color.FromArgb(100, 116, 139),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        // برچسب (راست‌چین و RTL)
        TextRenderer.DrawText(g, Hint, _fHint, new Rectangle(14, 3, Width - 64, 20),
            Color.FromArgb(100, 116, 139),
            TextFormatFlags.Right | TextFormatFlags.RightToLeft | TextFormatFlags.VerticalCenter |
            TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
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
            _fIcon.Dispose();
            _fHint.Dispose();
            _tb.Font?.Dispose();
        }
        base.Dispose(disposing);
    }
}
