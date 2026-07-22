using CeKeymap.Core.Localization;
using CeKeymap.Core.Models;

namespace CeKeymap.App.Infrastructure
{
    /// <summary>
    /// Process-wide accessor for the resolved <see cref="LocalizationCatalog"/>. Initialized once
    /// in the composition root before any UI is constructed.
    /// </summary>
    internal static class Loc
    {
        private static LocalizationCatalog _catalog = new LocalizationCatalog(null, null);

        public static void Initialize(LocalizationCatalog catalog)
        {
            _catalog = catalog;
        }

        public static string Get(string key) => _catalog.Get(key);

        public static string Get(string key, params object[] args) => _catalog.Get(key, args);

        public static string FeatureLabel(FeatureId featureId)
        {
            switch (featureId)
            {
                case FeatureId.AppWindowSwitch: return Get("feature.appWindowSwitch");
                case FeatureId.ZoomDesktop: return Get("feature.zoomDesktop");
                case FeatureId.ZoomMobile: return Get("feature.zoomMobile");
                case FeatureId.PressWinKey: return Get("feature.pressWinKey");
                default: return featureId.ToString();
            }
        }
    }
}
