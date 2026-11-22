namespace DNSphere;

partial class AboutForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        body = new BodyPanel();
        card = new BoxPanel();
        logo = new Label();
        lblName = new Label();
        lblSlogan = new Label();
        lblVersion = new Label();
        lblDesc = new Label();
        btnClose = new IconButton();

        body.SuspendLayout();
        card.SuspendLayout();
        SuspendLayout();

        // ---------- بدنه (زیر نوار عنوان Material) ----------
        body.Location = new Point(2, 64);
        body.Size = new Size(480, 470);
        body.TabIndex = 0;

        // ---------- کارت ----------
        card.FillColor = Color.FromArgb(240, 245, 255);
        card.BorderColor = Color.FromArgb(191, 207, 240);
        card.Radius = 16;
        card.Location = new Point(20, 20);
        card.Size = new Size(440, 372);
        card.TabIndex = 0;

        logo.AutoSize = false;
        logo.Text = Icons.Globe;
        logo.TextAlign = ContentAlignment.MiddleCenter;
        logo.BackColor = Color.FromArgb(63, 81, 181);
        logo.ForeColor = Color.White;
        logo.Location = new Point(184, 20);
        logo.Size = new Size(72, 72);

        lblName.AutoSize = false;
        lblName.Text = "DNSphere";
        lblName.TextAlign = ContentAlignment.MiddleCenter;
        lblName.BackColor = Color.FromArgb(240, 245, 255);
        lblName.ForeColor = Color.FromArgb(30, 41, 59);
        lblName.Location = new Point(10, 104);
        lblName.Size = new Size(420, 28);

        lblSlogan.AutoSize = false;
        lblSlogan.Text = "Ignite Your Network. Discover. Analyze. Optimize.";
        lblSlogan.TextAlign = ContentAlignment.MiddleCenter;
        lblSlogan.BackColor = Color.FromArgb(240, 245, 255);
        lblSlogan.ForeColor = Color.FromArgb(63, 81, 181);
        lblSlogan.Location = new Point(10, 134);
        lblSlogan.Size = new Size(420, 24);

        lblVersion.AutoSize = false;
        lblVersion.Text = "Version 1.0.0";
        lblVersion.TextAlign = ContentAlignment.MiddleCenter;
        lblVersion.BackColor = Color.FromArgb(240, 245, 255);
        lblVersion.ForeColor = Color.FromArgb(100, 116, 139);
        lblVersion.Location = new Point(10, 160);
        lblVersion.Size = new Size(420, 24);

        lblDesc.AutoSize = false;
        lblDesc.RightToLeft = RightToLeft.Yes;
        lblDesc.TextAlign = ContentAlignment.TopLeft;   // در RTL یعنی سمت راست
        lblDesc.BackColor = Color.FromArgb(240, 245, 255);
        lblDesc.ForeColor = Color.FromArgb(51, 65, 85);
        lblDesc.Location = new Point(24, 198);
        lblDesc.Size = new Size(392, 160);
        lblDesc.Text =
            "DNSphere ابزاری برای مشاهده و تنظیم سریع DNS روی کارت‌های شبکه‌ی فیزیکی است.\r\n\r\n" +
            "• نمایش کارت‌های شبکه‌ی فعال و غیرفعال و DNS تنظیم‌شده‌ی هرکدام\r\n" +
            "• انتخاب خودکار کارت متصل به اینترنت\r\n" +
            "• لیست DNSهای عمومی با قابلیت بروزرسانی آنلاین\r\n" +
            "• پینگ و مرتب‌سازی DNSها بر اساس سرعت\r\n" +
            "• پاک‌سازی کش DNS (Flush DNS)";

        // ---------- دکمه‌ی بستن ----------
        btnClose.Glyph = Icons.Check;
        btnClose.Primary = true;
        btnClose.Text = "بستن";
        btnClose.Radius = 17;
        btnClose.Location = new Point(160, 414);
        btnClose.Size = new Size(160, 36);
        btnClose.TabIndex = 1;
        btnClose.Click += btnClose_Click;

        // ---------- فرم ----------
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(484, 536);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "درباره‌ی DNSphere";

        card.Controls.Add(logo);
        card.Controls.Add(lblName);
        card.Controls.Add(lblSlogan);
        card.Controls.Add(lblVersion);
        card.Controls.Add(lblDesc);

        body.Controls.Add(card);
        body.Controls.Add(btnClose);
        Controls.Add(body);

        card.ResumeLayout(false);
        body.ResumeLayout(false);
        ResumeLayout(false);
    }

    private BodyPanel body;
    private BoxPanel card;
    private Label logo;
    private Label lblName;
    private Label lblSlogan;
    private Label lblVersion;
    private Label lblDesc;
    private IconButton btnClose;
}
