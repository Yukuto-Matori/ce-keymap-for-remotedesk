using CeKeymap.Core.Hotkeys;
using CeKeymap.Core.Models;
using NUnit.Framework;

namespace CeKeymap.Core.Tests.Hotkeys
{
    public class HotkeyMatcherTests
    {
        private readonly HotkeyMatcher _matcher = new HotkeyMatcher();

        [Test]
        public void Match_ExactModifiersAndMainKey_ReturnsFeatureId()
        {
            var settings = AppSettings.CreateDefault(); // ZoomDesktop = RAlt + D

            var result = _matcher.Match(new[] { ModifierKey.RAlt }, "D", settings);

            Assert.That(result, Is.EqualTo(FeatureId.ZoomDesktop));
        }

        [Test]
        public void Match_MainKeyCaseInsensitive_StillMatches()
        {
            var settings = AppSettings.CreateDefault();

            var result = _matcher.Match(new[] { ModifierKey.RAlt }, "d", settings);

            Assert.That(result, Is.EqualTo(FeatureId.ZoomDesktop));
        }

        [Test]
        public void Match_DisabledFeature_ReturnsNull()
        {
            var settings = AppSettings.CreateDefault();
            settings.Features[FeatureId.ZoomDesktop].Enabled = false;

            var result = _matcher.Match(new[] { ModifierKey.RAlt }, "D", settings);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Match_ExtraUnrelatedModifierPressed_ReturnsNull()
        {
            var settings = AppSettings.CreateDefault();

            var result = _matcher.Match(new[] { ModifierKey.RAlt, ModifierKey.RCtrl }, "D", settings);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Match_NoRegisteredComboMatches_ReturnsNull()
        {
            var settings = AppSettings.CreateDefault();

            var result = _matcher.Match(new[] { ModifierKey.LCtrl }, "Q", settings);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Match_ModifierOnlyCombo_MatchesWhenNoMainKeyPressed()
        {
            var settings = AppSettings.CreateDefault(); // AppWindowSwitch = RAlt + RShift, no main key

            var result = _matcher.Match(new[] { ModifierKey.RAlt, ModifierKey.RShift }, null, settings);

            Assert.That(result, Is.EqualTo(FeatureId.AppWindowSwitch));
        }

        [Test]
        public void Match_ModifierOnlyCombo_DoesNotMatchWhenExtraMainKeyPressed()
        {
            var settings = AppSettings.CreateDefault();

            var result = _matcher.Match(new[] { ModifierKey.RAlt, ModifierKey.RShift }, "X", settings);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Match_FeatureWithNullKeyCombo_NeverMatches()
        {
            var settings = AppSettings.CreateDefault();
            settings.Features[FeatureId.PressWinKey].KeyCombo = null;

            var result = _matcher.Match(new ModifierKey[0], null, settings);

            Assert.That(result, Is.Null);
        }
    }
}
