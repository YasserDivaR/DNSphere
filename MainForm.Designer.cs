namespace DNSphere;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlBody = new BodyPanel();
        pnlBottom = new SolidPanel();
        pnlAbout = new SolidPanel();
        lblAdapters = new Label();
        frameAdapters = new BoxPanel();
        flowAdapters = new ThemeFlowPanel();
        lblDnsList = new Label();
        btnPingAll = new IconButton();
        btnDnsRefresh = new IconButton();
        frameDns = new BoxPanel();
        flowDns = new ThemeFlowPanel();
        boxActions = new BoxPanel();
        txtDns1 = new IconTextBox();
        txtDns2 = new IconTextBox();
        btnDefault = new IconButton();
        btnRefresh = new IconButton();
        btnReset = new IconButton();
        btnFlush = new IconButton();
        btnManage = new IconButton();
        btnAbout = new IconButton();
        btnApply = new IconButton();
        statusStrip = new StatusStrip();
        lblStatusIcon = new ToolStripStatusLabel();
        lblStatus = new ToolStripStatusLabel();
        pnlBody.SuspendLayout();
        boxActions.SuspendLayout();
        SuspendLayout();

        // ---------- بدنه (زیر نوار عنوان Material) ----------
        pnlBody.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pnlBody.Location = new Point(2, 64);
        pnlBody.Size = new Size(968, 752);
        pnlBody.TabIndex = 0;

        // ---------- سربرگ: شعار برنامه + دکمه درباره ----------
        btnAbout.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        btnAbout.AccentColor = Color.FromArgb(63, 81, 181);
        btnAbout.Radius = 15;
        btnAbout.Glyph = Icons.Info;
        btnAbout.Text = "درباره";
        btnAbout.Location = new Point(16, 5);
        btnAbout.Size = new Size(104, 32);
        btnAbout.TabIndex = 7;
        btnAbout.Click += btnAbout_Click;

        // نوار پایین: دکمه‌ی درباره (چپ) + نوار وضعیت
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Height = 42;
        pnlBottom.FillColor = Color.FromArgb(224, 233, 250);

        pnlAbout.Dock = DockStyle.Left;
        pnlAbout.Width = 128;
        pnlAbout.FillColor = Color.FromArgb(224, 233, 250);


        // ---------- کارت‌های شبکه ----------
        lblAdapters.AutoSize = false;
        lblAdapters.RightToLeft = RightToLeft.Yes;
        lblAdapters.TextAlign = ContentAlignment.MiddleLeft;   // در حالت RTL، MiddleLeft یعنی سمت راست
        lblAdapters.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblAdapters.Size = new Size(758, 24);
        lblAdapters.BackColor = Color.Transparent;
        lblAdapters.ForeColor = Color.FromArgb(91, 75, 200);
        lblAdapters.Location = new Point(194, 12);
        lblAdapters.Text = "کارت‌های شبکه";

        // قاب مدرن دور لیست کارت‌های شبکه (آبی)
        frameAdapters.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        frameAdapters.FillColor = Color.FromArgb(240, 245, 255);
        frameAdapters.BorderColor = Color.FromArgb(191, 207, 240);
        frameAdapters.Radius = 16;
        frameAdapters.Location = new Point(16, 40);
        frameAdapters.Size = new Size(936, 262);
        frameAdapters.TabIndex = 0;

        flowAdapters.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        flowAdapters.AutoScroll = true;
        flowAdapters.FillColor = Color.FromArgb(240, 245, 255);   // همان رنگ قاب
        flowAdapters.FlowDirection = FlowDirection.TopDown;
        flowAdapters.WrapContents = false;
        flowAdapters.Padding = new Padding(4, 8, 0, 0);
        flowAdapters.Location = new Point(12, 6);
        flowAdapters.Size = new Size(924, 250);
        flowAdapters.TabIndex = 0;

        // ---------- لیست DNS ----------
        lblDnsList.AutoSize = false;
        lblDnsList.RightToLeft = RightToLeft.Yes;
        lblDnsList.TextAlign = ContentAlignment.MiddleLeft;
        lblDnsList.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblDnsList.Size = new Size(150, 24);
        lblDnsList.BackColor = Color.Transparent;
        lblDnsList.ForeColor = Color.FromArgb(13, 148, 136);
        lblDnsList.Location = new Point(802, 320);
        lblDnsList.Text = "DNSهای عمومی";

        btnDnsRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        btnDnsRefresh.AccentColor = Color.FromArgb(13, 148, 136);
        btnDnsRefresh.Primary = true;
        btnDnsRefresh.Radius = 17;
        btnDnsRefresh.Glyph = Icons.Download;
        btnDnsRefresh.Text = "بروزرسانی لیست DNS";
        btnDnsRefresh.Location = new Point(16, 314);
        btnDnsRefresh.Size = new Size(190, 36);
        btnDnsRefresh.TabIndex = 1;
        btnDnsRefresh.Click += btnDnsRefresh_Click;

        btnPingAll.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        btnPingAll.AccentColor = Color.FromArgb(13, 148, 136);
        btnPingAll.Primary = false;
        btnPingAll.Radius = 17;
        btnPingAll.Glyph = Icons.Speed;
        btnPingAll.Text = "پینگ همه و مرتب‌سازی";
        btnPingAll.Location = new Point(214, 314);
        btnPingAll.Size = new Size(206, 36);
        btnPingAll.TabIndex = 2;
        btnPingAll.Click += btnPingAll_Click;

        btnManage.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        btnManage.AccentColor = Color.FromArgb(13, 148, 136);
        btnManage.Primary = false;
        btnManage.Radius = 17;
        btnManage.Glyph = Icons.Edit;
        btnManage.Text = "مدیریت لیست";
        btnManage.Location = new Point(428, 314);
        btnManage.Size = new Size(150, 36);
        btnManage.TabIndex = 8;
        btnManage.Click += btnManage_Click;

        // قاب مدرن دور لیست DNSها (سبز-آبی)
        frameDns.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        frameDns.FillColor = Color.FromArgb(235, 248, 246);
        frameDns.BorderColor = Color.FromArgb(168, 222, 214);
        frameDns.Radius = 16;
        frameDns.Location = new Point(16, 358);
        frameDns.Size = new Size(936, 198);
        frameDns.TabIndex = 3;

        flowDns.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        flowDns.AutoScroll = true;
        flowDns.FillColor = Color.FromArgb(235, 248, 246);        // همان رنگ قاب
        flowDns.FlowDirection = FlowDirection.LeftToRight;
        flowDns.WrapContents = true;
        flowDns.Padding = new Padding(4, 8, 0, 0);
        flowDns.Location = new Point(12, 6);
        flowDns.Size = new Size(924, 186);
        flowDns.TabIndex = 0;

        // ---------- جعبه‌ی عملیات پایین ----------
        boxActions.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        boxActions.FillColor = Color.FromArgb(243, 239, 255);        // بنفش روشن
        boxActions.BorderColor = Color.FromArgb(205, 193, 243);
        boxActions.Radius = 16;
        boxActions.Location = new Point(16, 566);
        boxActions.Size = new Size(936, 134);
        boxActions.TabIndex = 4;

        txtDns1.Hint = "DNS اول (Preferred)";
        txtDns1.Location = new Point(16, 14);
        txtDns1.Size = new Size(250, 48);
        txtDns1.TabIndex = 0;

        txtDns2.Hint = "DNS دوم (Alternate)";
        txtDns2.Location = new Point(282, 14);
        txtDns2.Size = new Size(250, 48);
        txtDns2.TabIndex = 1;

        btnDefault.Glyph = Icons.Restore;
        btnDefault.Text = "مقادیر پیش‌فرض";
        btnDefault.Location = new Point(548, 20);
        btnDefault.Size = new Size(284, 36);
        btnDefault.TabIndex = 2;
        btnDefault.Click += btnDefault_Click;

        // دکمه‌ی بروزرسانی کارت‌ها: بالا، سمت چپ بخش کارت‌های شبکه (هم‌رنگ کارت‌های شبکه)
        btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        btnRefresh.AccentColor = Color.FromArgb(37, 99, 235);
        btnRefresh.Primary = false;
        btnRefresh.Radius = 14;
        btnRefresh.Glyph = Icons.Refresh;
        btnRefresh.Text = "بروزرسانی لیست";
        btnRefresh.Location = new Point(16, 6);
        btnRefresh.Size = new Size(170, 30);
        btnRefresh.TabIndex = 3;
        btnRefresh.Click += btnRefresh_Click;

        btnFlush.Glyph = Icons.Flush;
        btnFlush.Text = "Flush DNS";
        btnFlush.Location = new Point(174, 76);
        btnFlush.Size = new Size(150, 42);
        btnFlush.TabIndex = 6;
        btnFlush.Click += btnFlush_Click;

        btnReset.Glyph = Icons.Settings;
        btnReset.Text = "DNS خودکار (DHCP)";
        btnReset.Location = new Point(332, 76);
        btnReset.Size = new Size(200, 42);
        btnReset.TabIndex = 4;
        btnReset.Click += btnReset_Click;

        btnApply.Glyph = Icons.Check;
        btnApply.Primary = true;
        btnApply.Text = "اعمال DNS";
        btnApply.Location = new Point(548, 76);
        btnApply.Size = new Size(284, 42);
        btnApply.TabIndex = 5;
        btnApply.Click += btnApply_Click;

        // ---------- نوار وضعیت (StatusBar) پایین فرم ----------
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.AutoSize = false;
        statusStrip.SizingGrip = false;
        statusStrip.GripStyle = ToolStripGripStyle.Hidden;
        statusStrip.RightToLeft = RightToLeft.Yes;
        statusStrip.Padding = new Padding(28, 4, 28, 4);
        // در حالت RTL اولین آیتم سمت راست قرار می‌گیرد: اول آیکون، بعد متن
        statusStrip.Items.Add(lblStatusIcon);
        statusStrip.Items.Add(lblStatus);
        statusStrip.TabIndex = 6;

        lblStatusIcon.Margin = new Padding(0, 3, 8, 2);
        lblStatusIcon.Text = "";

        // Spring: برچسب کل عرض نوار را می‌گیرد. در حالت RightToLeft مقدار MiddleLeft
        // متن را در سمت راست نمایش می‌دهد (WinForms تراز را در RTL آینه می‌کند).
        lblStatus.Spring = true;
        lblStatus.RightToLeft = RightToLeft.Yes;
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        lblStatus.Text = "";

        // ---------- فرم ----------
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(972, 818);
        MinimumSize = new Size(972, 804);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "DNSphere";

        boxActions.Controls.Add(txtDns1);
        boxActions.Controls.Add(txtDns2);
        boxActions.Controls.Add(btnDefault);
        boxActions.Controls.Add(btnFlush);
        boxActions.Controls.Add(btnReset);
        boxActions.Controls.Add(btnApply);

        pnlBody.Controls.Add(btnRefresh);
        pnlBody.Controls.Add(lblAdapters);
        frameAdapters.Controls.Add(flowAdapters);
        frameDns.Controls.Add(flowDns);

        pnlBody.Controls.Add(frameAdapters);
        pnlBody.Controls.Add(lblDnsList);
        pnlBody.Controls.Add(btnPingAll);
        pnlBody.Controls.Add(btnManage);
        pnlBody.Controls.Add(btnDnsRefresh);
        pnlBody.Controls.Add(frameDns);
        pnlBody.Controls.Add(boxActions);
        pnlAbout.Controls.Add(btnAbout);
        pnlBottom.Controls.Add(statusStrip);   // اول Fill
        pnlBottom.Controls.Add(pnlAbout);      // بعد Left تا اول جای خودش را بگیرد
        pnlBody.Controls.Add(pnlBottom);

        Controls.Add(pnlBody);

        boxActions.ResumeLayout(false);
        pnlBody.ResumeLayout(false);
        pnlBody.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private BodyPanel pnlBody;
    private BoxPanel boxActions;
    private BoxPanel frameAdapters;
    private BoxPanel frameDns;
    private Label lblAdapters;
    private SolidPanel pnlBottom;
    private SolidPanel pnlAbout;
    private ThemeFlowPanel flowAdapters;
    private Label lblDnsList;
    private IconButton btnPingAll;
    private IconButton btnDnsRefresh;
    private ThemeFlowPanel flowDns;
    private IconTextBox txtDns1;
    private IconTextBox txtDns2;
    private IconButton btnDefault;
    private IconButton btnRefresh;
    private IconButton btnReset;
    private IconButton btnFlush;
    private IconButton btnManage;
    private IconButton btnAbout;
    private IconButton btnApply;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatusIcon;
    private ToolStripStatusLabel lblStatus;
}
