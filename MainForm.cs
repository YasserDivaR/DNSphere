using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace DNSphere;

public partial class MainForm : ReaLTaiizor.Forms.MaterialForm
{
    private const string Slogan = "Ignite Your Network. Discover. Analyze. Optimize.";
    private static readonly Color AdaptersLabelColor = Color.FromArgb(91, 75, 200);   // بنفش، هم‌خانواده با قاب کارت‌های شبکه
    private const string DefaultDns1 = "8.8.4.4";
    private const string DefaultDns2 = "1.0.0.3";

    private readonly List<AdapterCard> _cards = new();
    private AdapterCard _selected;
    private StatusRenderer statusRenderer;

    private readonly List<DnsCard> _dnsCards = new();
    private DnsCard _selectedDns;
    private bool _suppressDnsEvent;
    private int _pingVersion;   // با هر ساخت دوباره‌ی لیست، نتیجه‌ی پینگ‌های قدیمی نادیده گرفته می‌شود

    public MainForm()
    {
        InitializeComponent();

        // این‌ها داخل Designer نیستند چون Designer ویژوال استودیو کدهای غیرکامپوننتی را در بازنویسی پاک می‌کند
        statusRenderer = new StatusRenderer(Color.FromArgb(224, 233, 250));
        statusStrip.Renderer = statusRenderer;
        lblStatusIcon.Font = Icons.GetFont(13F);

        // فعال‌سازی تم Material روی این فرم
        ReaLTaiizor.Manager.MaterialSkinManager.Instance.AddFormToManage(this);

        // فونت یکسان برای همه‌ی بخش‌ها (Vazirmatn از طریق کلاس UiFont)
        UiFont.TryApplyToMaterialSkin(ReaLTaiizor.Manager.MaterialSkinManager.Instance);
        UiFont.ApplyTo(this);
        HookLabels();
        Shown += (s, e) =>
        {
            UiFont.TryApplyToMaterialSkin(ReaLTaiizor.Manager.MaterialSkinManager.Instance);
            UiFont.ApplyTo(this);   // تم Material ممکن است فونت‌ها را بعد از ساخت فرم برگردانده باشد
            FixLabelColors();
            Invalidate(true);
        };

        ApplyIcons();
        SetupScrollBars();

        // کاهش پرش و فریز هنگام تغییر اندازه + بازرسم حاشیه با هر تغییر اندازه
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw, true);

        txtDns1.Text = DefaultDns1;
        txtDns2.Text = DefaultDns2;
        txtDns1.TextChanged += DnsTextChanged;
        txtDns2.TextChanged += DnsTextChanged;

        boxActions.SizeChanged += (s, e) => LayoutActions();
        LayoutActions();
        new ToolTip().SetToolTip(lblDnsList, "برای انتخاب و پینگ، روی هر کارت کلیک کنید");

        flowAdapters.SizeChanged += (s, e) => ResizeCards();
        flowDns.SizeChanged += (s, e) => ResizeDnsCards();

        Load += (s, e) =>
        {
            LoadAdapters();
            // DNS_List.txt کنار برنامه (اگر وجود داشته باشد)، وگرنه لیست پیش‌فرض داخلی
            BuildDnsCards(DnsListService.LoadAll());
        };
    }

    /// <summary>
    /// چیدمان جعبه‌ی پایین بر اساس عرض فعلی، تا با تغییر اندازه‌ی فرم کنترل‌ها روی هم نیفتند.
    /// ردیف اول: دو فیلد DNS + «مقادیر پیش‌فرض»؛ ردیف دوم: چهار دکمه (دکمه‌ی «اعمال» باقی عرض را می‌گیرد).
    /// </summary>
    private void LayoutActions()
    {
        const int pad = 16, gap = 16;
        int w = boxActions.ClientSize.Width;
        if (w < 400) return;

        boxActions.SuspendLayout();

        // ستون راست (چسبیده به لبه‌ی راست): «مقادیر پیش‌فرض» و «اعمال DNS» هم‌اندازه
        const int rightW = 220;
        int rx = w - pad - rightW;
        btnDefault.SetBounds(rx, 20, rightW, 36);
        btnApply.SetBounds(rx, 76, rightW, 42);

        // بخش چپ با ابعاد ثابت (با تغییر اندازه‌ی فرم عوض نمی‌شود):
        // هر دکمه‌ی ردیف دوم هم‌عرض و هم‌تراز با فیلد DNS بالای خودش
        const int txtW = 280;
        int x2 = pad + txtW + gap;
        txtDns1.SetBounds(pad, 14, txtW, 48);
        txtDns2.SetBounds(x2, 14, txtW, 48);
        btnFlush.SetBounds(pad, 76, txtW, 42);
        btnReset.SetBounds(x2, 76, txtW, 42);

        boxActions.ResumeLayout(true);
    }

    /// <summary>حاشیه‌ی ۲ پیکسلی دور فرم اصلی (هم‌رنگ نوار عنوان)</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // شعار برنامه: وسط نوار عنوان، کنار نام برنامه (بالای بدنه، زیر دکمه‌های کنترل پنجره)
        using (var f = new Font(UiFont.Get(10F).FontFamily, 9F, FontStyle.Regular))
        {
            var bar = new Rectangle(0, 24, ClientSize.Width, 40);
            TextRenderer.DrawText(e.Graphics, Slogan, f, bar, Color.FromArgb(235, 255, 255, 255),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        }
        using var pen = new Pen(Color.FromArgb(63, 81, 181), 2F) { Alignment = System.Drawing.Drawing2D.PenAlignment.Inset };
        var r = ClientRectangle;
        e.Graphics.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
    }

    /// <summary>
    /// تم Material پس‌زمینه‌ی برچسب‌ها را خاکستری می‌کند. اینجا پس‌زمینه‌ی هر برچسب
    /// هم‌رنگ گرادیان بدنه در همان ارتفاع می‌شود و اگر تم دوباره آن را عوض کرد، برمی‌گردد.
    /// </summary>
    private void HookLabels()
    {
        foreach (var l in new[] { lblAdapters, lblDnsList })
        {
            l.BackColorChanged += (s, e) => FixLabelColors();
            l.LocationChanged += (s, e) => FixLabelColors();
        }
        lblAdapters.ForeColorChanged += (s, e) => FixLabelColors();
        lblDnsList.ForeColorChanged += (s, e) => FixLabelColors();
        pnlBody.SizeChanged += (s, e) => FixLabelColors();
        FixLabelColors();
    }

    private void FixLabelColors()
    {
        foreach (var l in new[] { lblAdapters, lblDnsList })
        {
            var c = pnlBody.ColorAtY(l.Top + l.Height / 2);
            if (l.BackColor != c) l.BackColor = c;
        }

        var f1 = AdaptersLabelColor;
        var f2 = Color.FromArgb(13, 148, 136);
        if (lblAdapters.ForeColor != f1) lblAdapters.ForeColor = f1;
        if (lblDnsList.ForeColor != f2) lblDnsList.ForeColor = f2;
    }

    /// <summary>نوار اسکرول مدرن (هم‌رنگ هر قاب) به جای نوار خاکستری پیش‌فرض ویندوز</summary>
    private void SetupScrollBars()
    {
        new ModernScrollBar { ThumbColor = Color.FromArgb(37, 99, 235) }.Attach(flowAdapters);
        new ModernScrollBar { ThumbColor = Color.FromArgb(13, 148, 136) }.Attach(flowDns);
    }

    /// <summary>آیکون پنجره؛ آیکون دکمه‌ها و فیلدها داخل خود کنترل‌ها رسم می‌شود</summary>
    private void ApplyIcons()
    {
        Icon = Icons.AppIcon(Color.FromArgb(37, 99, 235));
    }

    // ---------------------------------------------------------------
    // کارت‌های شبکه
    // ---------------------------------------------------------------
    private void LoadAdapters()
    {
        string previousId = _selected?.Adapter.Id;

        try
        {
            flowAdapters.SuspendLayout();
            foreach (var c in flowAdapters.Controls.Cast<Control>().ToList())
            {
                flowAdapters.Controls.Remove(c);
                c.Dispose();
            }
            _cards.Clear();
            _selected = null;

            var adapters = NetworkHelper.GetPhysicalAdapters();
            foreach (var a in adapters)
            {
                var card = new AdapterCard(a);
                card.Click += Card_Click;
                flowAdapters.Controls.Add(card);
                _cards.Add(card);
            }

            ResizeCards();
            flowAdapters.ResumeLayout(true);

            if (_cards.Count == 0)
            {
                SetStatus("هیچ کارت شبکه فیزیکی پیدا نشد.", true);
                return;
            }

            // انتخاب: کارت قبلی، وگرنه کارت فعالِ متصل به اینترنت، وگرنه اولی
            var target = _cards.FirstOrDefault(c => c.Adapter.Id == previousId)
                      ?? _cards.FirstOrDefault(c => c.Adapter.IsConnected)
                      ?? _cards[0];
            SelectCard(target);

            SetStatus($"{_cards.Count} کارت شبکه فیزیکی پیدا شد.", false);
        }
        catch (Exception ex)
        {
            flowAdapters.ResumeLayout(true);
            SetStatus("خطا در خواندن کارت‌های شبکه: " + ex.Message, true);
        }
    }

    private void ResizeCards()
    {
        // عرض ثابت (جدا از دیده‌شدن یا نشدن نوار اسکرول که زیر نوار سفارشی پنهان است)
        int w = Math.Max(300, flowAdapters.Width - SystemInformation.VerticalScrollBarWidth - 6 - flowAdapters.Padding.Left);
        if (_cards.All(c => c.Width == w)) return;   // بدون تغییر = بدون کار اضافه

        flowAdapters.SuspendLayout();
        foreach (var c in _cards) c.Width = w;
        flowAdapters.ResumeLayout(true);
    }

    private void Card_Click(object sender, EventArgs e)
    {
        if (sender is not AdapterCard card) return;
        SelectCard(card);

        // DNSهای تنظیم‌شده‌ی این کارت در فیلدها؛ اگر تنظیم نشده بود، خالی
        var servers = card.Adapter.DnsServers ?? new List<string>();
        txtDns1.Text = servers.Count > 0 ? servers[0] : "";
        txtDns2.Text = servers.Count > 1 ? servers[1] : "";
    }

    private void SelectCard(AdapterCard card)
    {
        foreach (var c in _cards) c.Selected = false;
        card.Selected = true;
        _selected = card;
    }

    private AdapterInfo SelectedAdapter() => _selected?.Adapter;

    // ---------------------------------------------------------------
    // لیست DNSهای عمومی
    // ---------------------------------------------------------------
    private void BuildDnsCards(List<DnsEntry> list)
    {
        flowDns.SuspendLayout();
        foreach (var c in flowDns.Controls.Cast<Control>().ToList())
        {
            flowDns.Controls.Remove(c);
            c.Dispose();
        }
        _dnsCards.Clear();
        _selectedDns = null;
        _pingVersion++;

        foreach (var entry in list)
        {
            var card = new DnsCard(entry);
            card.Click += DnsCard_Click;
            flowDns.Controls.Add(card);
            _dnsCards.Add(card);
        }

        ResizeDnsCards();
        flowDns.ResumeLayout(true);

        // اگر مقادیر فعلی فیلدها با یکی از کارت‌ها یکی بود، آن را انتخاب کن
        SyncDnsSelectionWithText();
    }

    /// <summary>کارت‌های DNS اندازه‌ی ثابت (۲۹۰×۶۲) دارند و فقط در ردیف‌ها کنار هم چیده می‌شوند</summary>
    private void ResizeDnsCards()
    {
        foreach (var c in _dnsCards)
            if (c.Width != 290) c.Width = 290;
    }

    private void DnsCard_Click(object sender, EventArgs e)
    {
        if (sender is not DnsCard card) return;

        _suppressDnsEvent = true;
        txtDns1.Text = card.Entry.Primary;
        txtDns2.Text = card.Entry.Secondary ?? "";
        _suppressDnsEvent = false;

        SelectDns(card);
        SetStatus($"DNS «{card.Entry.Name}» انتخاب شد. برای اعمال، «اعمال DNS» را بزنید.", false);

        // پینگ DNS اولِ همین کارت
        _ = PingCardAsync(card);
    }

    private async Task PingCardAsync(DnsCard card)
    {
        int ver = _pingVersion;
        card.SetPingMeasuring();
        long ms = await PingHelper.AverageAsync(card.Entry.Primary);
        if (ver != _pingVersion || card.IsDisposed) return;
        card.SetPing(ms);
    }

    /// <summary>پینگ همه‌ی DNSها (هم‌زمان) و مرتب‌سازی بر اساس کمترین پینگ</summary>
    private async void btnPingAll_Click(object sender, EventArgs e)
    {
        if (_dnsCards.Count == 0) return;

        int ver = _pingVersion;
        var cards = _dnsCards.ToList();

        btnPingAll.Enabled = false;
        btnDnsRefresh.Enabled = false;
        SetStatus("در حال پینگ همه‌ی DNSها...", false);

        try
        {
            using var gate = new SemaphoreSlim(8);
            await Task.WhenAll(cards.Select(async c =>
            {
                await gate.WaitAsync();
                try
                {
                    c.SetPingMeasuring();
                    long ms = await PingHelper.AverageAsync(c.Entry.Primary, 2, 1200);
                    if (ver == _pingVersion && !c.IsDisposed) c.SetPing(ms);
                }
                finally { gate.Release(); }
            }));

            if (ver != _pingVersion) return;

            SortByPing();
            int ok = cards.Count(c => c.PingMs >= 0);
            SetStatus($"پینگ تمام شد: {ok} از {cards.Count} DNS پاسخ دادند؛ لیست بر اساس کمترین پینگ مرتب شد.", ok == 0);
        }
        finally
        {
            btnPingAll.Enabled = true;
            btnDnsRefresh.Enabled = true;
        }
    }

    private void SortByPing()
    {
        // بدون پاسخ‌ها آخر لیست؛ ترتیب برابرها حفظ می‌شود
        var sorted = _dnsCards
            .OrderBy(c => c.PingMs >= 0 ? c.PingMs : long.MaxValue)
            .ToList();

        flowDns.SuspendLayout();
        for (int i = 0; i < sorted.Count; i++)
            flowDns.Controls.SetChildIndex(sorted[i], i);
        flowDns.ResumeLayout(true);
        flowDns.AutoScrollPosition = new Point(0, 0);

        _dnsCards.Clear();
        _dnsCards.AddRange(sorted);
    }

    private void SelectDns(DnsCard card)
    {
        foreach (var c in _dnsCards) c.Selected = false;
        if (card != null) card.Selected = true;
        _selectedDns = card;
    }

    /// <summary>اگر کاربر مقدار فیلدها را دستی تغییر داد، انتخاب کارت DNS برداشته می‌شود.</summary>
    private void DnsTextChanged(object sender, EventArgs e)
    {
        if (_suppressDnsEvent) return;
        SyncDnsSelectionWithText();
    }

    private void SyncDnsSelectionWithText()
    {
        string p = txtDns1.Text.Trim();
        string s = txtDns2.Text.Trim();
        var match = _dnsCards.FirstOrDefault(c =>
            c.Entry.Primary == p && (c.Entry.Secondary ?? "") == s);
        SelectDns(match);
    }

    private async void btnDnsRefresh_Click(object sender, EventArgs e)
        => await RefreshDnsListAsync(true);

    private async Task RefreshDnsListAsync(bool userInitiated)
    {
        btnDnsRefresh.Enabled = false;
        SetStatus("در حال دریافت لیست DNS از اینترنت...", false);
        try
        {
            var online = await DnsListService.DownloadAndSaveAsync();
            BuildDnsCards(DnsListService.LoadAll());   // لیست شخصی کاربر دست‌نخورده می‌ماند
            SetStatus($"لیست DNS بروزرسانی شد ({online.Count} مورد) و در DNS_List.txt کنار برنامه ذخیره شد.", false);
        }
        catch (Exception ex)
        {
            SetStatus("بروزرسانی لیست DNS ناموفق بود؛ لیست قبلی حفظ شد. (" + ex.Message + ")", true);
        }
        finally
        {
            btnDnsRefresh.Enabled = true;
        }
    }

    // ---------------------------------------------------------------
    // اعمال DNS
    // ---------------------------------------------------------------
    private static bool IsValidIPv4(string s, out string normalized)
    {
        normalized = null;
        s = s?.Trim();
        if (string.IsNullOrEmpty(s)) return false;
        if (s.Split('.').Length != 4) return false;
        if (!IPAddress.TryParse(s, out var ip)) return false;
        if (ip.AddressFamily != AddressFamily.InterNetwork) return false;
        normalized = ip.ToString();
        return true;
    }

    private void btnApply_Click(object sender, EventArgs e)
    {
        var adapter = SelectedAdapter();
        if (adapter == null)
        {
            SetStatus("لطفا یک کارت شبکه را انتخاب کنید.", true);
            return;
        }

        if (!IsValidIPv4(txtDns1.Text, out string d1))
        {
            SetStatus("DNS اول معتبر نیست.", true);
            txtDns1.Focus();
            return;
        }

        var list = new List<string> { d1 };
        if (!string.IsNullOrWhiteSpace(txtDns2.Text))
        {
            if (!IsValidIPv4(txtDns2.Text, out string d2))
            {
                SetStatus("DNS دوم معتبر نیست.", true);
                txtDns2.Focus();
                return;
            }
            list.Add(d2);
        }

        // بدون پنجره‌ی تایید؛ هشدار کارت غیرفعال در نوار وضعیت نمایش داده می‌شود
        bool wasDown = !adapter.IsUp;

        Cursor = Cursors.WaitCursor;
        var (ok, msg) = NetworkHelper.SetDns(adapter, list.ToArray());
        Cursor = Cursors.Default;
        LoadAdapters();
        if (ok && wasDown) msg += " (توجه: این کارت شبکه غیرفعال است.)";
        SetStatus(msg, !ok);
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
        var adapter = SelectedAdapter();
        if (adapter == null)
        {
            SetStatus("لطفا یک کارت شبکه را انتخاب کنید.", true);
            return;
        }

        Cursor = Cursors.WaitCursor;
        var (ok, msg) = NetworkHelper.SetDns(adapter, null);
        Cursor = Cursors.Default;
        LoadAdapters();
        SetStatus(ok ? "DNS روی حالت خودکار (DHCP) برگشت." : msg, !ok);
    }

    private void btnDefault_Click(object sender, EventArgs e)
    {
        txtDns1.Text = DefaultDns1;
        txtDns2.Text = DefaultDns2;
    }

    private void btnRefresh_Click(object sender, EventArgs e) => LoadAdapters();

    private async void btnFlush_Click(object sender, EventArgs e)
    {
        btnFlush.Enabled = false;
        SetStatus("در حال پاک‌سازی کش DNS...", false);
        try
        {
            var (ok, msg) = await Task.Run(NetworkHelper.FlushDnsCache);
            SetStatus(msg, !ok);
        }
        finally { btnFlush.Enabled = true; }
    }

    private void btnManage_Click(object sender, EventArgs e)
    {
        using var f = new DnsManagerForm();
        f.ShowDialog(this);
        if (!f.Changed) return;

        BuildDnsCards(DnsListService.LoadAll());
        SetStatus("لیست DNS شخصی ذخیره شد و لیست بروزرسانی شد.", false);
    }

    private void btnAbout_Click(object sender, EventArgs e)
    {
        using var f = new AboutForm();
        f.ShowDialog(this);
    }

    // هر دنباله‌ی لاتین (نام DNS، IP، نام فایل، پیام خطای انگلیسی) را یک واحد چپ‌به‌راست می‌کند
    private static readonly Regex LatinRun = new(
        @"[A-Za-z0-9](?:[A-Za-z0-9_.:/\\']*[A-Za-z0-9_/\\'])?(?:[ ]+(?:[-–][ ]+)?[A-Za-z0-9](?:[A-Za-z0-9_.:/\\']*[A-Za-z0-9_/\\'])?)*",
        RegexOptions.Compiled);

    /// <summary>
    /// در متن راست‌به‌چپ، بخش‌های انگلیسی (مثل «Us - Google Public Dns») ترتیب‌شان به‌هم می‌ریزد.
    /// با قرار دادن هر بخش داخل LRE...PDF ترتیب درست نمایش داده می‌شود.
    /// </summary>
    internal static string FixBidi(string s) =>
        string.IsNullOrEmpty(s) ? s : LatinRun.Replace(s, m => "\u202A" + m.Value + "\u202C");

    private void SetStatus(string text, bool error)
    {
        bool empty = string.IsNullOrWhiteSpace(text);
        var color = error ? Color.FromArgb(185, 28, 28) : Color.FromArgb(22, 130, 60);

        lblStatus.Text = FixBidi(text);
        lblStatus.ForeColor = color;
        lblStatusIcon.ForeColor = color;
        lblStatusIcon.Text = empty ? "" : (error ? Icons.Warning : Icons.Check);

        statusRenderer.Kind = empty ? StatusKind.Neutral : (error ? StatusKind.Error : StatusKind.Ok);
        statusStrip.Invalidate();
    }
}
