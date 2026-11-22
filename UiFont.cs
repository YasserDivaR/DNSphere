using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DNSphere;

/// <summary>
/// تنها مرجع فونت برنامه: Vazirmatn.
///
/// ترتیب پیدا کردن فونت:
///   1) اگر Vazirmatn روی ویندوز نصب باشد از همان استفاده می‌شود.
///   2) در غیر این صورت فایل‌های Vazirmatn*.ttf / .otf کنار برنامه (یا داخل پوشه‌ی Fonts کنار آن، یا
///      جاسازی‌شده در exe) پیدا می‌شوند، روی سیستم کاربر نصب می‌شوند (اولویت با نصب برای همه‌ی کاربران
///      چون برنامه با دسترسی Administrator اجرا می‌شود، وگرنه نصب برای کاربر فعلی) و برای همین اجرا
///      هم به‌صورت خصوصی بارگذاری می‌شوند تا نیازی به راه‌اندازی دوباره نباشد.
///   3) اگر هیچ‌کدام نبود Segoe UI و در نهایت فونت پیش‌فرض ویندوز.
///
/// همه‌ی کنترل‌ها فونتشان را فقط از Get() می‌گیرند و ApplyTo() فونت کنترل‌های استاندارد
/// (Label، TextBox و ...) را یکدست می‌کند.
/// </summary>
public static class UiFont
{
    public const string PreferredName = "Vazirmatn";

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);
    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern int AddFontResource(string lpszFilename);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam,
        uint flags, uint timeout, out IntPtr result);

    private const uint FR_PRIVATE = 0x10;
    private const uint WM_FONTCHANGE = 0x001D;
    private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);

    private static readonly PrivateFontCollection PrivateFonts = new();
    private static readonly List<FontFamily> Candidates = new();
    private static FontFamily _family;
    private static bool _resolved;

    /// <summary>آیا خود Vazirmatn (نصب‌شده یا بارگذاری‌شده) پیدا شد؟</summary>
    public static bool IsPreferredLoaded { get; private set; }

    private static FontFamily Family
    {
        get
        {
            if (!_resolved) Resolve();
            return _family;
        }
    }

    private static bool IsVazir(FontFamily f) => f.Name.Contains("vazir", StringComparison.OrdinalIgnoreCase);

    private static void Resolve()
    {
        _resolved = true;

        // 1) نصب‌شده روی ویندوز
        using (var installed = new InstalledFontCollection())
        {
            foreach (var f in installed.Families)
                if (IsVazir(f)) Candidates.Add(f);
        }

        // 2) نصب نبود: فایل کنار برنامه را نصب و بارگذاری کن
        if (Candidates.Count == 0)
        {
            var files = FindFontFiles();
            foreach (var file in files) TryInstall(file);
            foreach (var file in files) RegisterFile(file);   // برای استفاده‌ی همین اجرا

            foreach (var f in PrivateFonts.Families)
                if (IsVazir(f)) Candidates.Add(f);
        }

        if (Candidates.Count > 0)
        {
            // خانواده‌ی اصلی «Vazirmatn»؛ نه نسخه‌های وزن‌دار مثل «Vazirmatn Thin» یا «Vazirmatn Black»
            _family = Candidates.FirstOrDefault(f => f.Name.Equals(PreferredName, StringComparison.OrdinalIgnoreCase)
                                                     && f.IsStyleAvailable(FontStyle.Regular))
                      ?? Candidates.FirstOrDefault(f => f.IsStyleAvailable(FontStyle.Regular))
                      ?? Candidates[0];
            IsPreferredLoaded = true;
            return;
        }

        // 3) جایگزین
        using var inst = new InstalledFontCollection();
        _family = inst.Families.FirstOrDefault(f => f.Name == "Segoe UI")
                  ?? (SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont).FontFamily;
    }

    private static bool IsFontFile(string f) =>
        f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".otf", StringComparison.OrdinalIgnoreCase);

    /// <summary>فایل‌های Vazirmatn* کنار برنامه، داخل پوشه‌ی Fonts کنار برنامه، و فایل‌های جاسازی‌شده در exe</summary>
    private static List<string> FindFontFiles()
    {
        var result = new List<string>();

        foreach (var dir in new[] { AppContext.BaseDirectory, Path.Combine(AppContext.BaseDirectory, "Fonts") })
        {
            try
            {
                if (!Directory.Exists(dir)) continue;
                result.AddRange(Directory.GetFiles(dir, "Vazirmatn*").Where(IsFontFile));
            }
            catch { }
        }

        if (result.Count == 0)
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                string tmp = Path.Combine(Path.GetTempPath(), "DNSphere_Fonts");
                foreach (var res in asm.GetManifestResourceNames().Where(IsFontFile))
                {
                    using var s = asm.GetManifestResourceStream(res);
                    if (s == null) continue;

                    Directory.CreateDirectory(tmp);
                    string path = Path.Combine(tmp, res);
                    if (!File.Exists(path) || new FileInfo(path).Length != s.Length)
                    {
                        using var fs = File.Create(path);
                        s.CopyTo(fs);
                    }
                    result.Add(path);
                }
            }
            catch { }
        }

        return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// نصب فونت روی ویندوز. اول برای همه‌ی کاربران (پوشه‌ی Windows\Fonts و HKLM)،
    /// اگر نشد برای کاربر فعلی (HKCU). خطا برنامه را متوقف نمی‌کند.
    /// </summary>
    private static void TryInstall(string file)
    {
        string name = Path.GetFileNameWithoutExtension(file);
        string type = file.EndsWith(".otf", StringComparison.OrdinalIgnoreCase) ? " (OpenType)" : " (TrueType)";
        string regName = name + type;
        const string key = @"Software\Microsoft\Windows NT\CurrentVersion\Fonts";

        try
        {
            string sysDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            string dest = Path.Combine(sysDir, Path.GetFileName(file));
            if (!File.Exists(dest)) File.Copy(file, dest, false);

            using (var k = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(key))
                k?.SetValue(regName, Path.GetFileName(file));

            AddFontResource(dest);
            Broadcast();
            return;
        }
        catch { }

        try
        {
            // نصب برای کاربر فعلی (ویندوز 10 نسخه‌ی 1809 به بعد)
            string userDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Windows\Fonts");
            Directory.CreateDirectory(userDir);
            string dest = Path.Combine(userDir, Path.GetFileName(file));
            if (!File.Exists(dest)) File.Copy(file, dest, false);

            using (var k = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(key))
                k?.SetValue(regName, dest);

            AddFontResource(dest);
            Broadcast();
        }
        catch { }
    }

    private static void Broadcast()
    {
        try { SendMessageTimeout(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero, 2, 1000, out _); }
        catch { }
    }

    private static void RegisterFile(string path)
    {
        try
        {
            // برای رسم با GDI (TextRenderer و کنترل‌های استاندارد)
            AddFontResourceEx(path, FR_PRIVATE, IntPtr.Zero);
            // برای رسم با GDI+
            PrivateFonts.AddFontFile(path);
        }
        catch { }
    }

    /// <summary>اندازه‌ی ثابت همه‌ی متن‌های برنامه (پوینت)</summary>
    public const float BaseSize = 10F;

    /// <summary>
    /// فونت یکسان برنامه. طبق طراحی، همه‌ی متن‌ها همیشه 10pt و Regular هستند؛
    /// پارامترهای size و style فقط برای سازگاری با کدهای قبلی مانده‌اند و نادیده گرفته می‌شوند.
    /// </summary>
    public static Font Get(float size = BaseSize, FontStyle style = FontStyle.Regular)
    {
        var fam = Family;
        try
        {
            return new Font(fam, BaseSize, FontStyle.Regular);
        }
        catch
        {
            return new Font(SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont, FontStyle.Regular);
        }
    }

    /// <summary>فونت همه‌ی کنترل‌های استاندارد زیر root را با حفظ اندازه و استایل یکی می‌کند.</summary>
    public static void ApplyTo(Control root)
    {
        root.Font = Get(root.Font.Size, root.Font.Style);
        ApplyToolStrip(root);
        ApplyChildren(root);
    }

    /// <summary>
    /// آیتم‌های ToolStrip/StatusStrip (مثل برچسب وضعیت) فونت را همیشه از والد نمی‌گیرند و ممکن است
    /// فونت پیش‌فرض ویندوز بمانند؛ پس فونت هر آیتم هم صریحا ست می‌شود (به‌جز آیتم‌های آیکون).
    /// </summary>
    private static void ApplyToolStrip(Control c)
    {
        if (c is not ToolStrip ts) return;

        string iconFamily;
        using (var f = Icons.GetFont(10F)) iconFamily = f.FontFamily.Name;

        foreach (ToolStripItem item in ts.Items)
        {
            if (item.Font.FontFamily.Name == iconFamily) continue;   // آیتم آیکون
            item.Font = Get(item.Font.Size, item.Font.Style);
        }
    }

    private static void ApplyChildren(Control parent)
    {
        foreach (Control c in parent.Controls)
        {
            // کنترل‌های سفارشی فونتشان را خودشان از Get() می‌گیرند
            if (c is AdapterCard || c is DnsCard || c is IconButton || c is IconTextBox) continue;

            c.Font = Get(c.Font.Size, c.Font.Style);
            ApplyToolStrip(c);
            ApplyChildren(c);
        }
    }

    /// <summary>
    /// تلاش برای عوض کردن فونت‌های داخلی تم Material (مثل عنوان پنجره که Roboto است).
    /// چون ساختار داخلی کتابخانه را نمی‌دانیم، هر فیلد/پراپرتی از نوع Font را به‌صورت جداگانه
    /// و بدون خطا عوض می‌کنیم.
    /// </summary>
    public static void TryApplyToMaterialSkin(object manager)
    {
        if (manager == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var type = manager.GetType();

        foreach (var f in type.GetFields(flags))
        {
            try
            {
                if (f.FieldType == typeof(Font) && f.GetValue(manager) is Font old)
                    f.SetValue(manager, Get(old.Size, old.Style));
            }
            catch { }
        }

        foreach (var p in type.GetProperties(flags))
        {
            try
            {
                if (p.PropertyType == typeof(Font) && p.CanRead && p.CanWrite &&
                    p.GetIndexParameters().Length == 0 && p.GetValue(manager) is Font old)
                    p.SetValue(manager, Get(old.Size, old.Style));
            }
            catch { }
        }
    }
}
