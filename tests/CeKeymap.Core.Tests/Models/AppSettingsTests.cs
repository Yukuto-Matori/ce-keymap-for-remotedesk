using CeKeymap.Core.Models;
using NUnit.Framework;

namespace CeKeymap.Core.Tests.Models
{
    public class AppSettingsTests
    {
        [Test]
        public void CreateDefault_CurrentVer_Is1()
        {
            var settings = AppSettings.CreateDefault();

            Assert.That(settings.Ver, Is.EqualTo(1));
        }

        [Test]
        public void CreateDefault_AutoStart_IsFalse()
        {
            var settings = AppSettings.CreateDefault();

            Assert.That(settings.AutoStart, Is.False);
        }

        [Test]
        public void CreateDefault_AppWindowSwitch_MatchesSpec()
        {
            var binding = AppSettings.CreateDefault().Features[FeatureId.AppWindowSwitch];

            Assert.That(binding.Enabled, Is.True);
            Assert.That(binding.KeyCombo.Modifiers, Is.EquivalentTo(new[] { ModifierKey.RAlt, ModifierKey.RShift }));
            Assert.That(binding.KeyCombo.MainKey, Is.Null);
        }

        [Test]
        public void CreateDefault_ZoomDesktop_MatchesSpec()
        {
            var binding = AppSettings.CreateDefault().Features[FeatureId.ZoomDesktop];

            Assert.That(binding.Enabled, Is.True);
            Assert.That(binding.KeyCombo.Modifiers, Is.EquivalentTo(new[] { ModifierKey.RAlt }));
            Assert.That(binding.KeyCombo.MainKey, Is.EqualTo("D"));
            Assert.That(binding.ZoomPercent, Is.EqualTo(100));
        }

        [Test]
        public void CreateDefault_ZoomMobile_MatchesSpec()
        {
            var binding = AppSettings.CreateDefault().Features[FeatureId.ZoomMobile];

            Assert.That(binding.Enabled, Is.True);
            Assert.That(binding.KeyCombo.Modifiers, Is.EquivalentTo(new[] { ModifierKey.RAlt }));
            Assert.That(binding.KeyCombo.MainKey, Is.EqualTo("M"));
            Assert.That(binding.ZoomPercent, Is.EqualTo(150));
        }

        [Test]
        public void CreateDefault_PressWinKey_MatchesSpec()
        {
            var binding = AppSettings.CreateDefault().Features[FeatureId.PressWinKey];

            Assert.That(binding.Enabled, Is.True);
            Assert.That(binding.KeyCombo.Modifiers, Is.EquivalentTo(new[] { ModifierKey.RAlt }));
            Assert.That(binding.KeyCombo.MainKey, Is.EqualTo("W"));
        }

        [Test]
        public void CreateDefault_ContainsAllFourFeatures()
        {
            var settings = AppSettings.CreateDefault();

            Assert.That(settings.Features.Count, Is.EqualTo(4));
        }

        [Test]
        public void CreateDefault_ReturnsIndependentInstancesOnEachCall()
        {
            var first = AppSettings.CreateDefault();
            var second = AppSettings.CreateDefault();

            first.Features[FeatureId.ZoomDesktop].Enabled = false;

            Assert.That(second.Features[FeatureId.ZoomDesktop].Enabled, Is.True);
        }
    }
}
