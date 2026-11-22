namespace DNSphere;

/// <summary>
/// فرم مدیریت لیست DNS شخصی (DNS_Custom.json).
/// DNSهای آنلاین فقط‌خواندنی‌اند؛ با انتخاب آن‌ها و زدن «افزودن» می‌شود نسخه‌ی شخصی ساخت.
/// </summary>
public partial class DnsManagerForm : ReaLTaiizor.Forms.MaterialForm
{
    private static readonly Color Teal = Color.FromArgb(13, 148, 136);

    private readonly List<DnsEntry> _custom;
    private List<DnsEntry> _online;
    private readonly List<DnsCard> _cards = new();
    private DnsCard _selected;
    private StatusRenderer statusRenderer;

    /// <summary>آیا لیست شخصی تغییر کرده است؟</summary>
    public bool Changed { get; private set; }

    public DnsManagerForm()
    {
        _custom = DnsListService.LoadCustom();
        _online = DnsListService.LoadLocal();

        InitializeComponent();   // چیدمان فرم در DnsManagerForm.Designer.cs

        // خارج از Designer، چون Designer کدهای غیرکامپوننتی را پاک می‌کند
        statusRenderer = new StatusRenderer(Color.FromArgb(224, 233, 250));
        statusStrip.Renderer = statusRenderer;
        lblStatusIcon.Font = Icons.GetFont(13F);

        // تم Material و فونت یکسان، مثل فرم اصلی
        ReaLTaiizor.Manager.MaterialSkinManager.Instance.AddFormToManage(this);
        UiFont.TryApplyToMaterialSkin(ReaLTaiizor.Manager.MaterialSkinManager.Instance);

        // حداقل اندازه (ناحیه‌ی کلاینت ۶۸۴×۶۸۶)
        MinimumSize = new Size(Width - ClientSize.Width + 684, Height - ClientSize.Height + 686);
        Icon = Icons.AppIcon(Color.FromArgb(37, 99, 235));
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);

        new ModernScrollBar { ThumbColor = Teal }.Attach(flow);

        // رنگ برچسب‌ها: پس‌زمینه هم‌رنگ گرادیان؛ تم Material نتواند آن را خاکستری کند
        var fixTitle = ThemeGuard.Pin(lblTitle, () => body.ColorAtY(lblTitle.Top + lblTitle.Height / 2), Teal);
        var fixInfo = ThemeGuard.Pin(lblInfo, () => body.ColorAtY(lblInfo.Top + lblInfo.Height / 2), Color.FromArgb(71, 85, 105));
        _fixLabels = () => { fixTitle(); fixInfo(); };
        body.SizeChanged += (s, e) => _fixLabels();

        KeyPreview = true;
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

        UiFont.ApplyTo(this);
        Shown += (s, e) =>
        {
            UiFont.TryApplyToMaterialSkin(ReaLTaiizor.Manager.MaterialSkinManager.Instance);
            UiFont.ApplyTo(this);
            _fixLabels();
            Invalidate(true);
        };

        UpdateButtons();
        Rebuild(null);
    }

    private Action _fixLabels;

    /// <summary>حاشیه‌ی ۲ پیکسلی دور فرم (هم‌رنگ نوار عنوان)، مثل فرم اصلی</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(Color.FromArgb(63, 81, 181), 2F) { Alignment = System.Drawing.Drawing2D.PenAlignment.Inset };
        var r = ClientRectangle;
        e.Graphics.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
    }

    private void btnAdd_Click(object sender, EventArgs e) => AddEntry();
    private void btnSave_Click(object sender, EventArgs e) => SaveEdit();
    private void btnDelete_Click(object sender, EventArgs e) => DeleteEntry();
    private void btnImport_Click(object sender, EventArgs e) => ImportJson();
    private void btnExport_Click(object sender, EventArgs e) => ExportJson();
    private void btnClose_Click(object sender, EventArgs e) => Close();

    // ---------- کارت‌ها ----------
    private void Rebuild(DnsEntry select)
    {
        flow.SuspendLayout();
        foreach (var c in flow.Controls.Cast<Control>().ToList()) { flow.Controls.Remove(c); c.Dispose(); }
        _cards.Clear();
        _selected = null;

        var names = new HashSet<string>(_custom.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
        var all = new List<DnsEntry>(_custom);
        all.AddRange(_online.Where(o => !names.Contains(o.Name)));

        foreach (var entry in all)
        {
            var card = new DnsCard(entry);
            card.Click += (s, e) => Select((DnsCard)s);
            flow.Controls.Add(card);
            _cards.Add(card);
        }
        ResizeCards();
        flow.ResumeLayout(true);

        var target = select == null ? null : _cards.FirstOrDefault(c => c.Entry.Name.Equals(select.Name, StringComparison.OrdinalIgnoreCase));
        if (target != null) Select(target); else UpdateButtons();
    }

    /// <summary>کارت‌ها اندازه‌ی ثابت (۲۹۰×۶۲) دارند؛ فقط در ردیف‌ها کنار هم چیده می‌شوند</summary>
    private void ResizeCards()
    {
        foreach (var c in _cards)
            if (c.Width != 290) c.Width = 290;
    }

    private void Select(DnsCard card)
    {
        if (_selected != null) _selected.Selected = false;
        _selected = card;
        card.Selected = true;

        txtName.Text = card.Entry.Name;
        txtNote.Text = card.Entry.Description ?? "";
        txtDns1.Text = card.Entry.Primary;
        txtDns2.Text = card.Entry.Secondary ?? "";
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool custom = _selected != null && _selected.Entry.IsCustom;
        btnSave.Enabled = custom;
        btnDelete.Enabled = custom;
        btnExport.Enabled = _custom.Count > 0;
    }

    // ---------- عملیات ----------
    private bool TryReadFields(out DnsEntry entry, string ignoreName)
    {
        entry = null;
        string name = txtName.Text.Trim();
        string d1 = txtDns1.Text.Trim();
        string d2 = txtDns2.Text.Trim();

        if (name.Length == 0) { Msg("نام را وارد کنید.", true); return false; }
        if (!DnsListService.IsIPv4(d1)) { Msg("DNS اول معتبر نیست.", true); return false; }
        if (d2.Length > 0 && !DnsListService.IsIPv4(d2)) { Msg("DNS دوم معتبر نیست.", true); return false; }
        if (_custom.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                             !c.Name.Equals(ignoreName, StringComparison.OrdinalIgnoreCase)))
        { Msg("در لیست شخصی، DNS دیگری با این نام وجود دارد.", true); return false; }

        entry = new DnsEntry
        {
            Name = name,
            Primary = d1,
            Secondary = d2.Length > 0 ? d2 : null,
            Description = string.IsNullOrWhiteSpace(txtNote.Text) ? null : txtNote.Text.Trim(),
            IsCustom = true
        };
        return true;
    }

    private void AddEntry()
    {
        if (!TryReadFields(out var entry, null)) return;
        _custom.Insert(0, entry);
        Commit(entry, "DNS به لیست شخصی اضافه شد.");
    }

    private void SaveEdit()
    {
        if (_selected == null || !_selected.Entry.IsCustom) return;
        string old = _selected.Entry.Name;
        if (!TryReadFields(out var entry, old)) return;

        int i = _custom.FindIndex(c => c.Name.Equals(old, StringComparison.OrdinalIgnoreCase));
        if (i < 0) return;
        _custom[i] = entry;
        Commit(entry, "تغییرات ذخیره شد.");
    }

    private void DeleteEntry()
    {
        if (_selected == null || !_selected.Entry.IsCustom) return;
        string name = _selected.Entry.Name;
        if (MessageBox.Show(this, $"«{name}» از لیست شخصی حذف شود؟", "حذف DNS",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

        _custom.RemoveAll(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        txtName.Text = txtNote.Text = txtDns1.Text = txtDns2.Text = "";
        Commit(null, "DNS حذف شد.");
    }

    private void ImportJson()
    {
        using var dlg = new OpenFileDialog { Filter = "JSON (*.json)|*.json", Title = "Import DNS list" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var items = DnsListService.ParseCustomJson(File.ReadAllText(dlg.FileName));
            if (items.Count == 0) { Msg("هیچ DNS معتبری در فایل پیدا نشد.", true); return; }

            int added = 0, updated = 0;
            foreach (var it in items)
            {
                int i = _custom.FindIndex(c => c.Name.Equals(it.Name, StringComparison.OrdinalIgnoreCase));
                if (i >= 0) { _custom[i] = it; updated++; }
                else { _custom.Add(it); added++; }
            }
            Commit(null, $"Import انجام شد: {added} مورد جدید، {updated} مورد بروزرسانی.");
        }
        catch (Exception ex)
        {
            Msg("فایل JSON معتبر نیست: " + ex.Message, true);
        }
    }

    private void ExportJson()
    {
        using var dlg = new SaveFileDialog { Filter = "JSON (*.json)|*.json", FileName = "DNS_Custom.json", Title = "Export DNS list" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            File.WriteAllText(dlg.FileName, DnsListService.ToCustomJson(_custom), new System.Text.UTF8Encoding(false));
            Msg($"{_custom.Count} مورد Export شد.", false);
        }
        catch (Exception ex) { Msg("خطا در ذخیره‌ی فایل: " + ex.Message, true); }
    }

    private void Commit(DnsEntry select, string message)
    {
        try
        {
            DnsListService.SaveCustom(_custom);
            Changed = true;
            Rebuild(select);
            Msg(message, false);
        }
        catch (Exception ex)
        {
            Msg("خطا در ذخیره‌ی DNS_Custom.json: " + ex.Message, true);
        }
    }

    private void Msg(string text, bool error)
    {
        bool empty = string.IsNullOrWhiteSpace(text);
        var color = error ? Color.FromArgb(185, 28, 28) : Color.FromArgb(22, 130, 60);

        lblStatus.Text = MainForm.FixBidi(text);
        lblStatus.ForeColor = color;
        lblStatusIcon.ForeColor = color;
        lblStatusIcon.Text = empty ? "" : (error ? Icons.Warning : Icons.Check);

        statusRenderer.Kind = empty ? StatusKind.Neutral : (error ? StatusKind.Error : StatusKind.Ok);
        statusStrip.Invalidate();
    }
}
