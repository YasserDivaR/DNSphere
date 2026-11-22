using System.Net.NetworkInformation;

namespace DNSphere;

public static class PingHelper
{
    /// <summary>میانگین پینگ (میلی‌ثانیه). اگر هیچ پاسخی نیامد، -1 برمی‌گرداند.</summary>
    public static async Task<long> AverageAsync(string ip, int count = 3, int timeoutMs = 1500)
    {
        long total = 0;
        int ok = 0;
        try
        {
            using var p = new Ping();
            for (int i = 0; i < count; i++)
            {
                try
                {
                    var r = await p.SendPingAsync(ip, timeoutMs);
                    if (r.Status == IPStatus.Success)
                    {
                        total += r.RoundtripTime;
                        ok++;
                    }
                }
                catch { }
            }
        }
        catch { }

        return ok == 0 ? -1 : Math.Max(1, total / ok);
    }
}
