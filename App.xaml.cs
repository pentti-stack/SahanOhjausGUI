using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace SahanOhjausGUI
{
    public partial class App : Application
    {
        private static Mutex? _mutex;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, "SahanOhjausGUI_SingleInstance", out bool omistaja);

            if (!omistaja)
            {
                // Ohjelma jo käynnissä — tuo etualalle
                var existing = Process.GetProcessesByName("SahanOhjausGUI")
                    .FirstOrDefault();
                if (existing != null)
                {
                    ShowWindow(existing.MainWindowHandle, 9);
                    SetForegroundWindow(existing.MainWindowHandle);
                }
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}