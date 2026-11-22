using System.Reflection;

namespace DNSphere;

/// <summary>فرم «درباره‌ی برنامه» (چیدمان در AboutForm.Designer.cs) با همان سبک Material فرم اصلی</summary>
public partial class AboutForm : ReaLTaiizor.Forms.MaterialForm
{
    private Action _pinLabels;

    public AboutForm()
    {
        InitializeComponent();

        ReaLTaiizor.Manager.MaterialSkinManager.Instance.AddFormToManage(this);
        UiFont.TryApplyToMaterialSkin(ReaLTaiizor.Manager.MaterialSkinManager.Instance);

        // اندازه‌ی ثابت
        MinimumSize = MaximumSize = Size;
        MaximizeBox = false;
        Icon = Icons.AppIcon(Color.FromArgb(37, 99, 235));

        var v = Assembly.GetExecutingAssembly().GetName().Version;
        lblVersion.Text = "Version " + (v == null ? "1.0.0" : $"{v.Major}.{v.Minor}.{v.Build}");

        // لوگوی گرد
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddEllipse(0, 0, logo.Width, logo.Height);
        logo.Region = new Region(path);

        // رنگ‌ها را در برابر تم Material ثابت نگه دار
        Color cardFill = card.FillColor;
        var pins = new List<Action>
        {
            ThemeGuard.Pin(logo, () => Color.FromArgb(63, 81, 181), Color.White),
            ThemeGuard.Pin(lblName, () => cardFill, Color.FromArgb(30, 41, 59)),
            ThemeGuard.Pin(lblSlogan, () => cardFill, Color.FromArgb(63, 81, 181)),
            ThemeGuard.Pin(lblVersion, () => cardFill, Color.FromArgb(100, 116, 139)),
            ThemeGuard.Pin(lblDesc, () => cardFill, Color.FromArgb(51, 65, 85)),
        };
        _pinLabels = () => pins.ForEach(p => p());

        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);

        KeyPreview = true;
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

        UiFont.ApplyTo(this);
        logo.Font = Icons.GetFont(30F);   // ApplyTo فونت آیکون را عوض می‌کند؛ دوباره ست می‌شود

        Shown += (s, e) =>
        {
            UiFont.TryApplyToMaterialSkin(ReaLTaiizor.Manager.MaterialSkinManager.Instance);
            UiFont.ApplyTo(this);
            logo.Font = Icons.GetFont(30F);
            _pinLabels();
            Invalidate(true);
        };
    }

    private void btnClose_Click(object sender, EventArgs e) => Close();

    /// <summary>حاشیه‌ی ۲ پیکسلی دور فرم (هم‌رنگ نوار عنوان)، مثل فرم اصلی</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(Color.FromArgb(63, 81, 181), 2F) { Alignment = System.Drawing.Drawing2D.PenAlignment.Inset };
        var r = ClientRectangle;
        e.Graphics.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
    }
}
