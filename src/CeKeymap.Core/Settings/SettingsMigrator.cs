using System.Collections.Generic;
using CeKeymap.Core.Models;

namespace CeKeymap.Core.Settings
{
    /// <summary>
    /// Fills in missing fields from defaults and decides whether the version-mismatch
    /// notification dialog should be shown, independent of any UI concerns.
    /// </summary>
    public sealed class SettingsMigrator
    {
        public MigrationResult Reconcile(AppSettings parsed)
        {
            var defaults = AppSettings.CreateDefault();
            var versionMismatch = parsed.Ver != AppSettings.CurrentVer;
            var isUnknownNewerVersion = parsed.Ver > AppSettings.CurrentVer;
            var anyFieldDefaulted = isUnknownNewerVersion;

            var features = new Dictionary<FeatureId, FeatureBinding>();
            foreach (var featureId in defaults.Features.Keys)
            {
                if (parsed.Features != null && parsed.Features.TryGetValue(featureId, out var binding))
                {
                    features[featureId] = binding;
                }
                else
                {
                    features[featureId] = defaults.Features[featureId].Clone();
                    anyFieldDefaulted = true;
                }
            }

            var reconciled = new AppSettings
            {
                Ver = AppSettings.CurrentVer,
                AutoStart = parsed.AutoStart,
                Features = features,
            };

            var warn = versionMismatch && anyFieldDefaulted;
            return new MigrationResult(reconciled, warn);
        }
    }
}
