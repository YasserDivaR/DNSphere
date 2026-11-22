using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace DNSphere;

public class AdapterInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Mac { get; set; }
    public bool IsUp { get; set; }
    public bool HasGateway { get; set; }
    public bool IsWireless { get; set; }
    public string Ipv4 { get; set; }
    public string Dns { get; set; }

    /// <summary>آدرس‌های DNS تنظیم‌شده (دستی، وگرنه DHCP)؛ خالی یعنی تنظیم نشده</summary>
    public List<string> DnsServers { get; set; } = new();

    /// <summary>کارت فعال که گیتوی دارد (به اینترنت/شبکه وصل است)</summary>
    public bool IsConnected => IsUp && HasGateway;
}

public static class NetworkHelper
{
    private const string NetClassGuid = "{4D36E972-E325-11CE-BFC1-08002BE10318}";

    // برای بررسی اینکه دستگاه واقعا روی سیستم حاضر است (نه کارت فراموش‌شده‌ی قبلی)
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    private static extern int CM_Locate_DevNodeW(out uint devInst, string deviceId, uint flags);

    private static readonly string[] VirtualKeywords =
    {
        "bluetooth", "vmware", "virtualbox", "hyper-v", "vethernet", "tap-windows",
        "wintun", "wireguard", "openvpn", "loopback", "npcap", "wan miniport"
    };

    /// <summary>
    /// همه‌ی کارت‌های شبکه‌ی فیزیکی و واقعی، چه فعال و چه غیرفعال.
    /// لیست از رجیستری ویندوز (Network Connections) خوانده می‌شود، چون کارت‌های
    /// غیرفعال (Disable) در NetworkInterface.GetAllNetworkInterfaces دیده نمی‌شوند.
    /// </summary>
    public static List<AdapterInfo> GetPhysicalAdapters()
    {
        var result = new List<AdapterInfo>();

        var live = new Dictionary<string, NetworkInterface>(StringComparer.OrdinalIgnoreCase);
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            live[ni.Id] = ni;

        var descriptions = ReadDriverDescriptions();

        using var root = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Network\{NetClassGuid}");
        if (root == null) return result;

        foreach (string id in root.GetSubKeyNames())
        {
            if (!id.StartsWith("{")) continue;

            using var conn = root.OpenSubKey(id + @"\Connection");
            string name = conn?.GetValue("Name") as string;
            string pnp = (conn?.GetValue("PnpInstanceID") as string ?? "").Trim();
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pnp)) continue;
            if (!IsPhysicalPnp(pnp)) continue;

            descriptions.TryGetValue(id, out string desc);
            desc ??= name;
            string descLower = desc.ToLowerInvariant();
            if (VirtualKeywords.Any(k => descLower.Contains(k))) continue;

            bool wirelessByRegistry = conn.GetValue("MediaSubType") is int mst && mst == 2;
            bool wirelessByName = descLower.Contains("wi-fi") || descLower.Contains("wifi") ||
                                  descLower.Contains("wireless") || descLower.Contains("802.11") ||
                                  descLower.Contains("wlan");

            if (live.TryGetValue(id, out var ni))
            {
                var t = ni.NetworkInterfaceType;
                if (t == NetworkInterfaceType.Loopback || t == NetworkInterfaceType.Tunnel) continue;

                var ip = ni.GetIPProperties();
                string ipv4 = ip.UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    ?.Address.ToString() ?? "-";
                bool hasGw = ip.GatewayAddresses.Any(g =>
                    g.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !g.Address.Equals(IPAddress.Any));
                string dns = string.Join(", ", ip.DnsAddresses
                    .Where(d => d.AddressFamily == AddressFamily.InterNetwork)
                    .Select(d => d.ToString()));

                result.Add(new AdapterInfo
                {
                    Id = id,
                    Name = ni.Name,
                    Description = ni.Description,
                    Mac = FormatMac(ni.GetPhysicalAddress()),
                    IsUp = ni.OperationalStatus == OperationalStatus.Up,
                    HasGateway = hasGw,
                    IsWireless = t == NetworkInterfaceType.Wireless80211 || wirelessByRegistry,
                    Ipv4 = ipv4,
                    Dns = GetConfiguredDns(id, dns),
                    DnsServers = GetDnsServers(id)
                });
            }
            else
            {
                // کارت غیرفعال (Disable) که در لیست NetworkInterface نیست؛
                // فقط اگر دستگاه واقعا روی سیستم حاضر باشد نمایش داده می‌شود
                if (!IsDevicePresent(pnp)) continue;

                result.Add(new AdapterInfo
                {
                    Id = id,
                    Name = name,
                    Description = desc,
                    Mac = "-",
                    IsUp = false,
                    HasGateway = false,
                    IsWireless = wirelessByRegistry || wirelessByName,
                    Ipv4 = "-",
                    Dns = GetConfiguredDns(id, null),
                    DnsServers = GetDnsServers(id)
                });
            }
        }

        return result
            .OrderByDescending(a => a.IsConnected)
            .ThenByDescending(a => a.IsUp)
            .ThenBy(a => a.Name)
            .ToList();
    }

    /// <summary>
    /// DNS تنظیم‌شده برای کارت، مستقیم از رجیستری (حتی اگر کارت غیرفعال باشد):
    /// اول DNS دستی (Static)، بعد DNS دریافتی از DHCP.
    /// </summary>
    private static string GetConfiguredDns(string id, string liveFallback)
    {
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(
                $@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{id}");

            var stat = SplitDns(k?.GetValue("NameServer") as string);
            if (stat.Count > 0) return string.Join(", ", stat) + " (Static)";

            var dhcp = SplitDns(k?.GetValue("DhcpNameServer") as string);
            if (dhcp.Count > 0) return string.Join(", ", dhcp) + " (DHCP)";
        }
        catch { }

        return string.IsNullOrEmpty(liveFallback) ? "-" : liveFallback;
    }

    /// <summary>فقط آدرس‌های IPv4 تنظیم‌شده: اول دستی، بعد DHCP</summary>
    private static List<string> GetDnsServers(string id)
    {
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(
                $@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{id}");
            var list = SplitDns(k?.GetValue("NameServer") as string);
            if (list.Count == 0) list = SplitDns(k?.GetValue("DhcpNameServer") as string);
            return list.Where(x => x.Contains('.')).ToList();
        }
        catch { return new List<string>(); }
    }

    private static List<string> SplitDns(string s) =>
        (s ?? "").Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
                 .Select(x => x.Trim()).Where(x => x.Length > 0).ToList();

    /// <summary>کارت‌های واقعی PCI/USB/... فیزیکی‌اند؛ مجازی، نرم‌افزاری و بلوتوث حذف می‌شوند.</summary>
    private static bool IsPhysicalPnp(string pnp)
    {
        pnp = pnp.ToUpperInvariant();
        if (pnp.StartsWith("ROOT\\")) return false;     // درایورهای مجازی (VMware, VPN, TAP, ...)
        if (pnp.StartsWith("SWD\\")) return false;      // دستگاه‌های نرم‌افزاری
        if (pnp.StartsWith("BTH")) return false;        // بلوتوث (BTH\ و BTHENUM\)
        if (pnp.StartsWith("VMBUS\\")) return false;    // Hyper-V
        if (pnp.StartsWith("VMS")) return false;        // Hyper-V Virtual Switch
        if (pnp.StartsWith("{")) return false;
        return true;
    }

    private static bool IsDevicePresent(string pnp)
    {
        try
        {
            // flags = 0 یعنی فقط دستگاه‌های موجود (نه فانتوم)
            return CM_Locate_DevNodeW(out _, pnp, 0) == 0;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>نام درایور هر کارت (مثلا Realtek PCIe GbE Family Controller) از رجیستری</summary>
    private static Dictionary<string, string> ReadDriverDescriptions()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var cls = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Class\{NetClassGuid}");
            if (cls == null) return map;

            foreach (string sub in cls.GetSubKeyNames())
            {
                if (sub.Length != 4 || !sub.All(char.IsDigit)) continue;
                using var k = cls.OpenSubKey(sub);
                string guid = k?.GetValue("NetCfgInstanceId") as string;
                string desc = k?.GetValue("DriverDesc") as string;
                if (!string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(desc))
                    map[guid] = desc;
            }
        }
        catch { }
        return map;
    }

    /// <summary>تنظیم DNS با netsh. اگر servers خالی باشد، DNS خودکار (DHCP) می‌شود.</summary>
    public static (bool ok, string message) SetDns(AdapterInfo adapter, string[] servers)
    {
        try
        {
            string name = adapter.Name;
            if (servers == null || servers.Length == 0)
            {
                RunNetsh($"interface ipv4 set dnsservers name=\"{name}\" source=dhcp");
            }
            else
            {
                RunNetsh($"interface ipv4 set dnsservers name=\"{name}\" static {servers[0]} primary validate=no");
                for (int i = 1; i < servers.Length; i++)
                    RunNetsh($"interface ipv4 add dnsservers name=\"{name}\" address={servers[i]} index={i + 1} validate=no");
            }
            FlushDns();
            return (true, "DNS با موفقیت تنظیم شد.");
        }
        catch (Exception ex)
        {
            return (false, "خطا در تنظیم DNS: " + ex.Message);
        }
    }

    private static void RunNetsh(string args)
    {
        var psi = new ProcessStartInfo("netsh", args)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var p = Process.Start(psi);
        string output = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0)
            throw new Exception(output.Trim());
    }

    /// <summary>پاک‌سازی کش DNS ویندوز (ipconfig /flushdns) با گزارش نتیجه</summary>
    public static (bool ok, string message) FlushDnsCache()
    {
        try
        {
            var psi = new ProcessStartInfo("ipconfig", "/flushdns")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var p = Process.Start(psi);
            if (p == null) return (false, "اجرای ipconfig ممکن نشد.");
            if (!p.WaitForExit(5000)) return (false, "پاک‌سازی کش DNS بیش از حد طول کشید.");
            return p.ExitCode == 0
                ? (true, "کش DNS با موفقیت پاک‌سازی شد (Flush DNS).")
                : (false, "پاک‌سازی کش DNS ناموفق بود (کد " + p.ExitCode + ").");
        }
        catch (Exception ex)
        {
            return (false, "خطا در پاک‌سازی کش DNS: " + ex.Message);
        }
    }

    private static void FlushDns()
    {
        try
        {
            var psi = new ProcessStartInfo("ipconfig", "/flushdns")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(psi)?.WaitForExit(3000);
        }
        catch { }
    }

    private static string FormatMac(PhysicalAddress pa)
    {
        var b = pa.GetAddressBytes();
        return b.Length == 0 ? "-" : string.Join("-", b.Select(x => x.ToString("X2")));
    }
}
