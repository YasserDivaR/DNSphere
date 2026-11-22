namespace DNSphere;

partial class DnsManagerForm
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
        lblTitle = new Label();
        lblInfo = new Label();
        frame = new BoxPanel();
        flow = new ThemeFlowPanel();
        editBox = new BoxPanel();
        tblEdit = new ThemeTablePanel();
        txtName = new IconTextBox();
        txtNote = new IconTextBox();
        txtDns1 = new IconTextBox();
        txtDns2 = new IconTextBox();
        btnAdd = new IconButton();
        btnSave = new IconButton();
        btnDelete = new IconButton();
        btnImport = new IconButton();
        btnExport = new IconButton();
        btnClose = new IconButton();
        statusStrip = new StatusStrip();
        lblStatusIcon = new ToolStripStatusLabel();
        lblStatus = new ToolStripStatusLabel();

        body.SuspendLayout();
        editBox.SuspendLayout();
        frame.SuspendLayout();
        tblEdit.SuspendLayout();
        SuspendLayout();

        // ---------- بدنه ----------
        // زیر نوار عنوان Material (۶۴ پیکسل) و با حاشیه‌ی ۲ پیکسلی، مثل فرم اصلی
        body.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        body.Location = new Point(2, 64);
        body.Size = new Size(680, 700);
        body.TabIndex = 0;

        // ---------- متن‌های بالا (راست‌چین: در RTL مقدار MiddleLeft یعنی سمت راست) ----------
        lblTitle.AutoSize = false;
        lblTitle.RightToLeft = RightToLeft.Yes;
        lblTitle.TextAlign = ContentAlignment.MiddleLeft;
        lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblTitle.BackColor = Color.Transparent;
        lblTitle.ForeColor = Color.FromArgb(13, 148, 136);
        lblTitle.Location = new Point(20, 12);
        lblTitle.Size = new Size(640, 26);
        lblTitle.Text = "لیست DNS شخصی شما";

        lblInfo.AutoSize = false;
        lblInfo.RightToLeft = RightToLeft.Yes;
        lblInfo.TextAlign = ContentAlignment.MiddleLeft;
        lblInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblInfo.BackColor = Color.Transparent;
        lblInfo.ForeColor = Color.FromArgb(71, 85, 105);
        lblInfo.Location = new Point(20, 38);
        lblInfo.Size = new Size(640, 22);
        lblInfo.Text = "کارت‌هایی با آیکون شخص، شخصی‌اند. DNSهای آنلاین را برای ویرایش، با «افزودن» کپی کنید.";

        // ---------- قاب لیست کارت‌ها ----------
        frame.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        frame.FillColor = Color.FromArgb(235, 248, 246);
        frame.BorderColor = Color.FromArgb(168, 222, 214);
        frame.Radius = 16;
        frame.Location = new Point(16, 68);
        frame.Size = new Size(648, 258);
        frame.TabIndex = 0;

        flow.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        flow.AutoScroll = true;
        flow.FillColor = Color.FromArgb(235, 248, 246);
        flow.FlowDirection = FlowDirection.LeftToRight;
        flow.WrapContents = true;
        flow.Padding = new Padding(4, 8, 0, 0);
        flow.Location = new Point(12, 6);
        flow.Size = new Size(636, 246);
        flow.TabIndex = 0;

        // ---------- جعبه‌ی ویرایش (بنفش) ----------
        editBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        editBox.FillColor = Color.FromArgb(243, 239, 255);
        editBox.BorderColor = Color.FromArgb(205, 193, 243);
        editBox.Radius = 16;
        editBox.Location = new Point(16, 338);
        editBox.Size = new Size(648, 262);
        editBox.TabIndex = 1;

        // جدول ۶ ستونه‌ی هم‌عرض: هر فیلد ۳ ستون، هر دکمه ۲ ستون؛ پس لبه‌ها همیشه هم‌تراز می‌ماند
        tblEdit.Dock = DockStyle.Fill;
        // رنگ ثابت هم‌رنگ جعبه (نه Transparent): دکمه‌ها و فیلدها گوشه‌هایشان را با رنگ والد پاک می‌کنند
        tblEdit.FillColor = Color.FromArgb(243, 239, 255);
        tblEdit.Padding = new Padding(16, 14, 16, 0);
        tblEdit.ColumnCount = 6;
        tblEdit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667F));
        tblEdit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667F));
        tblEdit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667F));
        tblEdit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667F));
        tblEdit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667F));
        tblEdit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66667F));
        tblEdit.RowCount = 4;
        tblEdit.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
        tblEdit.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
        tblEdit.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tblEdit.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tblEdit.TabIndex = 0;

        // فیلدها
        txtName.Dock = DockStyle.Fill;
        txtName.Hint = "نام (Name)";
        txtName.Glyph = Icons.Person;
        txtName.Margin = new Padding(0, 0, 8, 14);
        txtName.TabIndex = 0;

        txtNote.Dock = DockStyle.Fill;
        txtNote.Hint = "توضیح (اختیاری)";
        txtNote.Glyph = Icons.Edit;
        txtNote.Margin = new Padding(8, 0, 0, 14);
        txtNote.TabIndex = 1;

        txtDns1.Dock = DockStyle.Fill;
        txtDns1.Hint = "DNS اول (Primary)";
        txtDns1.Margin = new Padding(0, 0, 8, 14);
        txtDns1.TabIndex = 2;

        txtDns2.Dock = DockStyle.Fill;
        txtDns2.Hint = "DNS دوم (Secondary، اختیاری)";
        txtDns2.Margin = new Padding(8, 0, 0, 14);
        txtDns2.TabIndex = 3;

        // دکمه‌ها: افزودن (راست) / ذخیره / حذف؛ Import و Export از چپ
        btnAdd.Dock = DockStyle.Fill;
        btnAdd.Glyph = Icons.Add;
        btnAdd.Primary = true;
        btnAdd.AccentColor = Color.FromArgb(13, 148, 136);
        btnAdd.Text = "افزودن";
        btnAdd.Margin = new Padding(11, 0, 0, 16);
        btnAdd.TabIndex = 4;
        btnAdd.Click += btnAdd_Click;

        btnSave.Dock = DockStyle.Fill;
        btnSave.Glyph = Icons.Check;
        btnSave.Text = "ذخیره‌ی ویرایش";
        btnSave.Margin = new Padding(5, 0, 5, 16);
        btnSave.TabIndex = 5;
        btnSave.Click += btnSave_Click;

        btnDelete.Dock = DockStyle.Fill;
        btnDelete.Glyph = Icons.Delete;
        btnDelete.AccentColor = Color.FromArgb(220, 38, 38);
        btnDelete.Text = "حذف";
        btnDelete.Margin = new Padding(0, 0, 11, 16);
        btnDelete.TabIndex = 6;
        btnDelete.Click += btnDelete_Click;

        btnImport.Dock = DockStyle.Fill;
        btnImport.Glyph = Icons.Download;
        btnImport.Text = "Import (JSON)";
        btnImport.Margin = new Padding(0, 0, 11, 16);
        btnImport.TabIndex = 7;
        btnImport.Click += btnImport_Click;

        btnExport.Dock = DockStyle.Fill;
        btnExport.Glyph = Icons.Upload;
        btnExport.Text = "Export (JSON)";
        btnExport.Margin = new Padding(5, 0, 5, 16);
        btnExport.TabIndex = 8;
        btnExport.Click += btnExport_Click;

        tblEdit.Controls.Add(txtName, 0, 0);   tblEdit.SetColumnSpan(txtName, 3);
        tblEdit.Controls.Add(txtNote, 3, 0);   tblEdit.SetColumnSpan(txtNote, 3);
        tblEdit.Controls.Add(txtDns1, 0, 1);   tblEdit.SetColumnSpan(txtDns1, 3);
        tblEdit.Controls.Add(txtDns2, 3, 1);   tblEdit.SetColumnSpan(txtDns2, 3);
        tblEdit.Controls.Add(btnDelete, 0, 2); tblEdit.SetColumnSpan(btnDelete, 2);
        tblEdit.Controls.Add(btnSave, 2, 2);   tblEdit.SetColumnSpan(btnSave, 2);
        tblEdit.Controls.Add(btnAdd, 4, 2);    tblEdit.SetColumnSpan(btnAdd, 2);
        tblEdit.Controls.Add(btnImport, 0, 3); tblEdit.SetColumnSpan(btnImport, 2);
        tblEdit.Controls.Add(btnExport, 2, 3); tblEdit.SetColumnSpan(btnExport, 2);

        // ---------- دکمه‌ی بستن (چسبیده به پایین، وسط) ----------
        btnClose.Anchor = AnchorStyles.Bottom;
        btnClose.Glyph = Icons.Check;
        btnClose.Text = "بستن";
        btnClose.Radius = 17;
        btnClose.Location = new Point(260, 612);
        btnClose.Size = new Size(160, 34);
        btnClose.TabIndex = 2;
        btnClose.Click += btnClose_Click;

        // ---------- نوار وضعیت (هم‌سبک فرم اصلی) ----------
        statusStrip.Dock = DockStyle.Bottom;
        statusStrip.AutoSize = false;
        statusStrip.Height = 42;
        statusStrip.SizingGrip = false;
        statusStrip.GripStyle = ToolStripGripStyle.Hidden;
        statusStrip.RightToLeft = RightToLeft.Yes;
        statusStrip.Padding = new Padding(28, 4, 28, 4);
        statusStrip.Items.Add(lblStatusIcon);
        statusStrip.Items.Add(lblStatus);
        statusStrip.TabIndex = 3;

        lblStatusIcon.Margin = new Padding(0, 3, 8, 2);
        lblStatusIcon.Text = "";

        lblStatus.Spring = true;
        lblStatus.RightToLeft = RightToLeft.Yes;
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        lblStatus.Text = "";

        // ---------- فرم ----------
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(684, 766);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "مدیریت لیست DNS";

        frame.Controls.Add(flow);
        editBox.Controls.Add(tblEdit);

        body.Controls.Add(lblTitle);
        body.Controls.Add(lblInfo);
        body.Controls.Add(frame);
        body.Controls.Add(editBox);
        body.Controls.Add(btnClose);
        body.Controls.Add(statusStrip);

        Controls.Add(body);

        tblEdit.ResumeLayout(false);
        frame.ResumeLayout(false);
        editBox.ResumeLayout(false);
        body.ResumeLayout(false);
        body.PerformLayout();
        ResumeLayout(false);
    }

    private BodyPanel body;
    private Label lblTitle;
    private Label lblInfo;
    private BoxPanel frame;
    private ThemeFlowPanel flow;
    private BoxPanel editBox;
    private ThemeTablePanel tblEdit;
    private IconTextBox txtName;
    private IconTextBox txtNote;
    private IconTextBox txtDns1;
    private IconTextBox txtDns2;
    private IconButton btnAdd;
    private IconButton btnSave;
    private IconButton btnDelete;
    private IconButton btnImport;
    private IconButton btnExport;
    private IconButton btnClose;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatusIcon;
    private ToolStripStatusLabel lblStatus;
}
