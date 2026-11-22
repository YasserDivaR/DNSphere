using System.Drawing.Drawing2D;

namespace DNSphere;

/// <summary>
/// نوار اسکرول باریک و گرد که دقیقا روی نوار اسکرول پیش‌فرض (خاکستری) ویندوز می‌نشیند
/// و آن را می‌پوشاند. باید با Attach به یک FlowLayoutPanel (با AutoScroll) وصل شود.
/// </summary>
public class ModernScrollBar : Control
{
    private FlowLayoutPanel _flow;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 50 };
    private bool _hover;
    private bool _drag;
    private int _dragOffset;
    private int _lastPos = -1, _lastContent = -1, _lastView = -1;

    public Color ThumbColor { get; set; } = Color.FromArgb(37, 99, 235);

    public ModernScrollBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        TabStop = false;
        Cursor = Cursors.Default;
        _timer.Tick += (s, e) =>
        {
            // وقتی اسکرول لازم نیست، نوار مخفی می‌شود تا روی کارت‌ها (سمت راست لیست) را نپوشاند
            bool show = TryMetrics(out _, out _, out _, out _);
            if (Visible != show)
            {
                Visible = show;
                if (show) BringToFront();
            }
            if (show && Changed()) Invalidate();
        };
    }

    /// <summary>اتصال به لیست؛ نوار خودش را کنار (روی) نوار پیش‌فرض آن قرار می‌دهد.</summary>
    public void Attach(FlowLayoutPanel flow)
    {
        _flow = flow;
        var host = flow.Parent;
        if (host != null && Parent != host) host.Controls.Add(this);

        flow.SizeChanged += (s, e) => Reposition();
        flow.LocationChanged += (s, e) => Reposition();
        Reposition();
        Visible = false;   // تا زمانی که لیست واقعا اسکرول نیاز داشته باشد مخفی است
        _timer.Start();
    }

    private void Reposition()
    {
        if (_flow == null) return;
        int w = SystemInformation.VerticalScrollBarWidth;
        Bounds = new Rectangle(_flow.Right - w, _flow.Top, w, _flow.Height);
        BringToFront();
    }

    private bool TryMetrics(out int content, out int view, out int pos, out int maxPos)
    {
        content = view = pos = maxPos = 0;
        if (_flow == null) return false;

        var vs = _flow.VerticalScroll;
        view = Math.Max(1, vs.LargeChange);
        content = vs.Maximum + 1;
        pos = vs.Value;
        maxPos = content - view;
        return vs.Visible && maxPos > 0;
    }

    private bool Changed()
    {
        TryMetrics(out int c, out int v, out int p, out _);
        if (c == _lastContent && v == _lastView && p == _lastPos) return false;
        _lastContent = c; _lastView = v; _lastPos = p;
        return true;
    }

    private Rectangle TrackRect() => new Rectangle(5, 6, Math.Max(4, Width - 10), Math.Max(10, Height - 12));

    private Rectangle ThumbRect(int content, int view, int pos, int maxPos)
    {
        var track = TrackRect();
        int thumbH = Math.Max(32, (int)(track.Height * (double)view / content));
        thumbH = Math.Min(thumbH, track.Height);
        int y = track.Y + (int)((track.Height - thumbH) * (double)pos / maxPos);
        return new Rectangle(track.X, y, track.Width, thumbH);
    }

    private void ScrollTo(int newPos, int maxPos)
    {
        newPos = Math.Max(0, Math.Min(maxPos, newPos));
        _flow.AutoScrollPosition = new Point(0, newPos);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        // رنگ زمینه‌ی لیست؛ نوار پیش‌فرض را کاملا می‌پوشاند
        g.Clear(_flow?.BackColor ?? Parent?.BackColor ?? SystemColors.Control);

        if (!TryMetrics(out int content, out int view, out int pos, out int maxPos)) return;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        var track = TrackRect();
        using (var tp = RoundRect(track, track.Width / 2))
        using (var tb = new SolidBrush(Color.FromArgb(34, ThumbColor)))
            g.FillPath(tb, tp);

        var thumb = ThumbRect(content, view, pos, maxPos);
        int alpha = _drag ? 255 : _hover ? 225 : 175;
        using (var th = RoundRect(thumb, thumb.Width / 2))
        using (var thb = new SolidBrush(Color.FromArgb(alpha, ThumbColor)))
            g.FillPath(thb, th);
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!TryMetrics(out int content, out int view, out int pos, out int maxPos)) return;

        var thumb = ThumbRect(content, view, pos, maxPos);
        if (thumb.Contains(e.Location))
        {
            _drag = true;
            _dragOffset = e.Y - thumb.Y;
        }
        else
        {
            // کلیک روی مسیر: یک صفحه بالا یا پایین
            ScrollTo(pos + (e.Y < thumb.Y ? -view : view) * 9 / 10, maxPos);
        }
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_drag) return;
        if (!TryMetrics(out int content, out int view, out int pos, out int maxPos)) return;

        var track = TrackRect();
        var thumb = ThumbRect(content, view, pos, maxPos);
        int range = track.Height - thumb.Height;
        if (range <= 0) return;

        int thumbY = e.Y - _dragOffset - track.Y;
        ScrollTo((int)Math.Round(maxPos * (double)thumbY / range), maxPos);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _drag = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (!TryMetrics(out _, out _, out int pos, out int maxPos)) return;
        ScrollTo(pos - e.Delta / 120 * 60, maxPos);
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
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}
