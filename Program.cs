using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DNSphere;

internal static class Program
{
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);

    private const int SW_RESTORE = 9;

    [STAThread]
    static void Main()
    {
        // فقط یک نمونه از برنامه اجازه‌ی اجرا دارد
        using var mutex = new Mutex(true, "DNSphere_SingleInstance_5E2B8C1A", out bool isFirstInstance);
        if (!isFirstInstance)
        {
            BringExistingToFront();
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());

        GC.KeepAlive(mutex);
    }

    /// <summary>اگر برنامه از قبل باز است، پنجره‌ی آن را جلو می‌آورد.</summary>
    private static void BringExistingToFront()
    {
        try
        {
            using var current = Process.GetCurrentProcess();
            foreach (var p in Process.GetProcessesByName(current.ProcessName))
            {
                using (p)
                {
                    if (p.Id == current.Id || p.MainWindowHandle == IntPtr.Zero) continue;

                    if (IsIconic(p.MainWindowHandle)) ShowWindow(p.MainWindowHandle, SW_RESTORE);
                    SetForegroundWindow(p.MainWindowHandle);
                    break;
                }
            }
        }
        catch { }
    }
}
