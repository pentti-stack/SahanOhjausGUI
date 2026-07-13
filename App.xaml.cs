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
                // Ohjelma jo käynnissä — hae prosessi ja tuo ikkuna esiin
                var existing = Process.GetProcessesByName("SahanOhjausGUI")
                    .FirstOrDefault();
                if (existing != null)
                {
                    IntPtr hWnd = existing.MainWindowHandle;
                    ShowWindow(hWnd, 9);   // SW_RESTORE — näyttää myös piilotetun
                    ShowWindow(hWnd, 3);   // SW_MAXIMIZE — maksimoi
                    SetForegroundWindow(hWnd);
                }
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}