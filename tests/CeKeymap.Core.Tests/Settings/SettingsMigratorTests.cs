using System.Collections.Generic;
using CeKeymap.Core.Models;
using CeKeymap.Core.Settings;
using NUnit.Framework;

namespace CeKeymap.Core.Tests.Settings
{
    public class SettingsMigratorTests
    {
        private readonly SettingsMigrator _migrator = new SettingsMigrator();

        [Test]
        public void Reconcile_CurrentVerAllFieldsPresent_NoWarningAndValuesPreserved()
        {
            var parsed = AppSettings.CreateDefault();
            parsed.Features[FeatureId.ZoomDesktop].ZoomPercent = 175;

            var result = _migrator.Reconcile(parsed);

            Assert.That(result.AppliedDefaultsDueToVersionMismatch, Is.False);
            Assert.That(result.Settings.Features[FeatureId.ZoomDesktop].ZoomPercent, Is.EqualTo(175));
        }

        [Test]
        public void Reconcile_OlderVerMissingFeatures_FillsDefaultsAndWarns()
        {
            var parsed = new AppSettings
            {
                Ver = 0, // older schema, unknown/absent Ver
                AutoStart = true,
                Features = new Dictionary<FeatureId, FeatureBinding>
                {
                    [FeatureId.AppWindowSwitch] = AppSettings.CreateDefault().Features[FeatureId.AppWindowSwitch],
                    // ZoomDesktop, ZoomMobile, PressWinKey absent - simulates an older settings.json
                }
            };

            var result = _migrator.Reconcile(parsed);

            Assert.That(result.AppliedDefaultsDueToVersionMismatch, Is.True);
            Assert.That(result.Settings.Features.Count, Is.EqualTo(4));
            Assert.That(result.Settings.Features[FeatureId.ZoomDesktop].KeyCombo.MainKey, Is.EqualTo("D"));
            Assert.That(result.Settings.Features[FeatureId.PressWinKey].Enabled, Is.True);
            Assert.That(result.Settings.AutoStart, Is.True);
        }

        [Test]
        public void Reconcile_UnknownNewerVer_TreatedAsWarningEvenIfFieldsPresent()
        {
            var parsed = AppSettings.CreateDefault();
            parsed.Ver = AppSettings.CurrentVer + 1;

            var result = _migrator.Reconcile(parsed);

            Assert.That(result.AppliedDefaultsDueToVersionMismatch, Is.True);
        }

        [Test]
        public void Reconcile_OlderVerButAllFieldsPresent_NoWarning()
        {
            var parsed = AppSettings.CreateDefault();
            parsed.Ver = AppSettings.CurrentVer - 1;

            var result = _migrator.Reconcile(parsed);

            Assert.That(result.AppliedDefaultsDueToVersionMismatch, Is.False);
        }

        [Test]
        public void Reconcile_AlwaysNormalizesVerToCurrent()
        {
            var parsed = AppSettings.CreateDefault();
            parsed.Ver = 0;

            var result = _migrator.Reconcile(parsed);

            Assert.That(result.Settings.Ver, Is.EqualTo(AppSettings.CurrentVer));
        }
    }
}
