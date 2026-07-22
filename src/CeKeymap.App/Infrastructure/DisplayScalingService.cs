using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace CeKeymap.App.Infrastructure
{
    /// <summary>
    /// Changes the current display's scaling ("拡大率") by writing the same registry values
    /// Windows' own Settings app uses (HKCU\Control Panel\Desktop\PerMonitorSettings\&lt;monitor&gt;\DpiValue,
    /// a signed step offset from the monitor's recommended scaling, in the same ~25%-wide steps
    /// as the Settings &gt; Display scaling slider), then locks the workstation: explorer.exe
    /// restart alone did not force Windows to re-apply the new value (confirmed by testing), but
    /// Windows does reliably re-read per-monitor DPI when a session resumes from the lock screen.
    /// </summary>
    internal sealed class DisplayScalingService
    {
        private const string PerMonitorSettingsPath = @"Control Panel\Desktop\PerMonitorSettings";
        private const int RecommendedScalingPercent = 100;
        private const int StepPercent = 25;

        private readonly FileLogger _logger;

        public DisplayScalingService(FileLogger logger)
        {
            _logger = logger;
        }

        public void ApplyZoomPercent(int zoomPercent)
        {
            var step = (int)Math.Round((zoomPercent - RecommendedScalingPercent) / (double)StepPercent, MidpointRounding.AwayFromZero);

            using (var perMonitorKey = Registry.CurrentUser.OpenSubKey(PerMonitorSettingsPath, writable: true))
            {
                if (perMonitorKey == null)
                {
                    _logger.LogError("PerMonitorSettings registry key was not found; cannot change display scaling.");
                    return;
                }

                foreach (var monitorName in perMonitorKey.GetSubKeyNames())
                {
                    using (var monitorKey = perMonitorKey.OpenSubKey(monitorName, writable: true))
                    {
                        monitorKey?.SetValue("DpiValue", step, RegistryValueKind.DWord);
                    }
                }
            }

            _logger.Log($"Wrote DpiValue={step} (ZoomPercent={zoomPercent}) to all monitors; locking workstation to apply.");

            if (!LockWorkStation())
            {
                _logger.LogError($"LockWorkStation failed (Win32 error {Marshal.GetLastWin32Error()}).");
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool LockWorkStation();
    }
}
