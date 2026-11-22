using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNSphere;

public class DnsEntry
{
    public string Name { get; set; }
    public string Primary { get; set; }
    public string Secondary { get; set; }

    [JsonPropertyName("note")]
    public string Description { get; set; }

    /// <summary>true یعنی از DNS_Custom.json (لیست شخصی کاربر) آمده است</summary>
    [JsonIgnore]
    public bool IsCustom { get; set; }
}

/// <summary>
/// لیست DNSهای عمومی.
///
/// - به‌صورت پیش‌فرض چند DNS داخل برنامه هست.
/// - فقط وقتی کاربر دکمه‌ی بروزرسانی را بزند، لیست از DownloadUrl دریافت می‌شود
///   و کنار فایل اجرایی در DNS_List.txt ذخیره می‌شود؛ از آن به بعد همین فایل خوانده می‌شود.
///
/// قالب هر خط:   نام=DNS اول,DNS دوم
/// مثال:         Us - Google Public Dns=8.8.8.8,8.8.4.4
/// </summary>
public static class DnsListService
{
    public const string DownloadUrl = "https://dl.yasserdivar.ir/DNS_List.txt";

    /// <summary>مسیر فایل کنار فایل اجرایی</summary>
    public static string LocalFilePath => Path.Combine(AppContext.BaseDirectory, "DNS_List.txt");

    /// <summary>فایل شخصی کاربر (فقط خود کاربر/فرم مدیریت آن را تغییر می‌دهد؛ بروزرسانی آنلاین به آن دست نمی‌زند)</summary>
    public static string CustomFilePath => Path.Combine(AppContext.BaseDirectory, "DNS_Custom.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>لیست آنلاین/پیش‌فرض + لیست شخصی. DNSهای شخصی اول می‌آیند و در نام تکراری اولویت دارند.</summary>
    public static List<DnsEntry> LoadAll()
    {
        var custom = LoadCustom();
        var names = new HashSet<string>(custom.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
        var all = new List<DnsEntry>(custom);
        all.AddRange(LoadLocal().Where(o => !names.Contains(o.Name)));
        return all;
    }

    /// <summary>خواندن DNS_Custom.json (ورودی‌های نامعتبر نادیده گرفته می‌شوند)</summary>
    public static List<DnsEntry> LoadCustom()
    {
        try { return ParseCustomJson(SafeRead(CustomFilePath)); }
        catch { return new List<DnsEntry>(); }   // JSON خراب → لیست شخصی خالی (برنامه از کار نمی‌افتد)
    }

    public static void SaveCustom(IEnumerable<DnsEntry> list) =>
        File.WriteAllText(CustomFilePath, ToCustomJson(list), new UTF8Encoding(false));

    public static string ToCustomJson(IEnumerable<DnsEntry> list) =>
        JsonSerializer.Serialize(list.Select(e => new DnsEntry
        {
            Name = e.Name,
            Primary = e.Primary,
            Secondary = string.IsNullOrWhiteSpace(e.Secondary) ? null : e.Secondary,
            Description = string.IsNullOrWhiteSpace(e.Description) ? null : e.Description
        }).ToList(), new JsonSerializerOptions(JsonOpts) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

    /// <summary>خواندن متن JSON (برای فایل شخصی و Import)؛ در صورت خراب بودن JSON استثنا می‌دهد</summary>
    public static List<DnsEntry> ParseCustomJson(string json)
    {
        var result = new List<DnsEntry>();
        if (string.IsNullOrWhiteSpace(json)) return result;

        var raw = JsonSerializer.Deserialize<List<DnsEntry>>(json, JsonOpts) ?? new();

        foreach (var e in raw)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.Name) || !IsIPv4(e.Primary)) continue;
            result.Add(new DnsEntry
            {
                Name = e.Name.Trim(),
                Primary = e.Primary.Trim(),
                Secondary = IsIPv4(e.Secondary) ? e.Secondary.Trim() : null,
                Description = string.IsNullOrWhiteSpace(e.Description) ? null : e.Description.Trim(),
                IsCustom = true
            });
        }
        return result;
    }

    private static string SafeRead(string path)
    {
        try { return File.Exists(path) ? File.ReadAllText(path) : null; }
        catch { return null; }
    }

    /// <summary>لیست پیش‌فرض داخلی برنامه</summary>
    public static List<DnsEntry> BuiltIn() => new()
    {
        new DnsEntry { Name = "Google Public DNS",   Primary = "8.8.8.8",        Secondary = "8.8.4.4" },
        new DnsEntry { Name = "Cloudflare",          Primary = "1.1.1.1",        Secondary = "1.0.0.1" },
        new DnsEntry { Name = "Cloudflare Security", Primary = "1.1.1.2",        Secondary = "1.0.0.2" },
        new DnsEntry { Name = "Cloudflare Family",   Primary = "1.1.1.3",        Secondary = "1.0.0.3" },
        new DnsEntry { Name = "Quad9",               Primary = "9.9.9.9",        Secondary = "149.112.112.112" },
        new DnsEntry { Name = "OpenDNS",             Primary = "208.67.222.222", Secondary = "208.67.220.220" },
        new DnsEntry { Name = "AdGuard DNS",         Primary = "94.140.14.14",   Secondary = "94.140.15.15" },
    };

    /// <summary>اگر DNS_List.txt کنار برنامه باشد از آن، وگرنه لیست داخلی</summary>
    public static List<DnsEntry> LoadLocal()
    {
        try
        {
            if (File.Exists(LocalFilePath))
            {
                var list = Parse(File.ReadAllText(LocalFilePath));
                if (list.Count > 0) return list;
            }
        }
        catch { }
        return BuiltIn();
    }

    /// <summary>دانلود لیست، ذخیره در DNS_List.txt کنار فایل اجرایی و برگرداندن لیست</summary>
    public static async Task<List<DnsEntry>> DownloadAndSaveAsync()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        string text = await http.GetStringAsync(DownloadUrl);

        var list = Parse(text);
        if (list.Count == 0)
            throw new InvalidDataException("فایل دانلودشده خالی یا نامعتبر است.");

        File.WriteAllText(LocalFilePath, text.TrimStart('﻿'), new System.Text.UTF8Encoding(false));
        return list;
    }

    /// <summary>هر خط: نام=DNS1,DNS2</summary>
    public static List<DnsEntry> Parse(string text)
    {
        var result = new List<DnsEntry>();
        if (string.IsNullOrEmpty(text)) return result;

        foreach (var raw in text.Split('\n'))
        {
            string line = raw.Trim().Trim('﻿').Trim();
            if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//")) continue;

            int eq = line.LastIndexOf('=');
            if (eq <= 0) continue;

            string name = line.Substring(0, eq).Trim();
            var addrs = line.Substring(eq + 1)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (name.Length == 0 || addrs.Length == 0 || !IsIPv4(addrs[0])) continue;

            result.Add(new DnsEntry
            {
                Name = name,
                Primary = addrs[0],
                Secondary = addrs.Length > 1 && IsIPv4(addrs[1]) ? addrs[1] : null
            });
        }
        return result;
    }

    public static bool IsIPv4(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        return s.Split('.').Length == 4 &&
               IPAddress.TryParse(s, out var ip) &&
               ip.AddressFamily == AddressFamily.InterNetwork;
    }
}
