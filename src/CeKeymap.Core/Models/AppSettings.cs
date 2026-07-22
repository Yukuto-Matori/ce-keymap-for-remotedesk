using System.Collections.Generic;

namespace CeKeymap.Core.Models
{
    public sealed class AppSettings
    {
        public const int CurrentVer = 1;

        public int Ver { get; set; } = CurrentVer;

        public bool AutoStart { get; set; }

        public Dictionary<FeatureId, FeatureBinding> Features { get; set; } = new Dictionary<FeatureId, FeatureBinding>();

        public static AppSettings CreateDefault()
        {
            return new AppSettings
            {
                Ver = CurrentVer,
                AutoStart = false,
                Features = new Dictionary<FeatureId, FeatureBinding>
                {
                    [FeatureId.AppWindowSwitch] = new FeatureBinding(
                        FeatureId.AppWindowSwitch,
                        enabled: true,
                        keyCombo: new KeyCombo(new[] { ModifierKey.RAlt, ModifierKey.RShift }, null)),
                    [FeatureId.ZoomDesktop] = new FeatureBinding(
                        FeatureId.ZoomDesktop,
                        enabled: true,
                        keyCombo: new KeyCombo(new[] { ModifierKey.RAlt }, "D"),
                        zoomPercent: 100),
                    [FeatureId.ZoomMobile] = new FeatureBinding(
                        FeatureId.ZoomMobile,
                        enabled: true,
                        keyCombo: new KeyCombo(new[] { ModifierKey.RAlt }, "M"),
                        zoomPercent: 150),
                    [FeatureId.PressWinKey] = new FeatureBinding(
                        FeatureId.PressWinKey,
                        enabled: true,
                        keyCombo: new KeyCombo(new[] { ModifierKey.RAlt }, "W")),
                }
            };
        }
    }
}
