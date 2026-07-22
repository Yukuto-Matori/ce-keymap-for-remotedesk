using System.Runtime.InteropServices;

namespace CeKeymap.App.Infrastructure
{
    /// <summary>
    /// Changes the current display's scaling ("拡大率") immediately, without sign-out.
    /// Uses SPI_SETLOGICALDPIOVERRIDE (undocumented but stable since Windows 10 1703, used by
    /// several public DPI-changing utilities): it takes a step count relative to the display's
    /// recommended scaling, in the same 25%-wide steps as the Settings > Display scaling slider.
    /// </summary>
    internal sealed class DisplayScalingService
    {
        private const uint SpiSetLogicalDpiOverride = 0x009E;
        private const uint SpifSendChange = 0x02;
        private const int RecommendedScalingPercent = 100;
        private const int StepPercent = 25;

        public void ApplyZoomPercent(int zoomPercent)
        {
            var step = (zoomPercent - RecommendedScalingPercent) / StepPercent;
            SystemParametersInfo(SpiSetLogicalDpiOverride, (uint)step, System.IntPtr.Zero, SpifSendChange);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, System.IntPtr pvParam, uint fWinIni);
    }
}
