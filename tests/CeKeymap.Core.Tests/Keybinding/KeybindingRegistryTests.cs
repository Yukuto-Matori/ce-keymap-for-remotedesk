using CeKeymap.Core.Keybinding;
using CeKeymap.Core.Models;
using NUnit.Framework;

namespace CeKeymap.Core.Tests.Keybinding
{
    public class KeybindingRegistryTests
    {
        private readonly KeybindingRegistry _registry = new KeybindingRegistry();

        [Test]
        public void Validate_NoModifiersWithMainKey_ReturnsFalse()
        {
            var combo = new KeyCombo(new ModifierKey[0], "D");

            Assert.That(_registry.Validate(combo), Is.False);
        }

        [Test]
        public void Validate_NoModifiersNoMainKey_ReturnsFalse()
        {
            var combo = new KeyCombo(new ModifierKey[0], null);

            Assert.That(_registry.Validate(combo), Is.False);
        }

        [Test]
        public void Validate_SingleModifierNoMainKey_ReturnsFalse()
        {
            var combo = new KeyCombo(new[] { ModifierKey.RShift }, null);

            Assert.That(_registry.Validate(combo), Is.False);
        }

        [Test]
        public void Validate_SingleModifierWithMainKey_ReturnsTrue()
        {
            var combo = new KeyCombo(new[] { ModifierKey.RAlt }, "D");

            Assert.That(_registry.Validate(combo), Is.True);
        }

        [Test]
        public void Validate_TwoModifiersNoMainKey_ReturnsTrue()
        {
            var combo = new KeyCombo(new[] { ModifierKey.RAlt, ModifierKey.RShift }, null);

            Assert.That(_registry.Validate(combo), Is.True);
        }

        [Test]
        public void SetBinding_UnusedCombo_AssignsAndBumpsNothing()
        {
            var settings = AppSettings.CreateDefault();
            var newCombo = new KeyCombo(new[] { ModifierKey.LCtrl }, "Z");

            var result = _registry.SetBinding(settings, FeatureId.PressWinKey, newCombo);

            Assert.That(settings.Features[FeatureId.PressWinKey].KeyCombo, Is.EqualTo(newCombo));
            Assert.That(settings.Features[FeatureId.PressWinKey].Enabled, Is.True);
            Assert.That(result.BumpedFeatureId, Is.Null);
        }

        [Test]
        public void SetBinding_ComboHeldByAnotherFeature_BumpsOtherFeatureOff()
        {
            var settings = AppSettings.CreateDefault();
            var zoomDesktopCombo = settings.Features[FeatureId.ZoomDesktop].KeyCombo; // RAlt+D

            var result = _registry.SetBinding(settings, FeatureId.PressWinKey, zoomDesktopCombo);

            Assert.That(result.BumpedFeatureId, Is.EqualTo(FeatureId.ZoomDesktop));
            Assert.That(settings.Features[FeatureId.ZoomDesktop].Enabled, Is.False);
            Assert.That(settings.Features[FeatureId.ZoomDesktop].KeyCombo, Is.Null);
            Assert.That(settings.Features[FeatureId.PressWinKey].KeyCombo, Is.EqualTo(zoomDesktopCombo));
            Assert.That(settings.Features[FeatureId.PressWinKey].Enabled, Is.True);
        }

        [Test]
        public void SetBinding_ReassignFeatureToItsOwnExistingCombo_DoesNotAffectOtherFeatures()
        {
            var settings = AppSettings.CreateDefault();
            var ownCombo = settings.Features[FeatureId.ZoomDesktop].KeyCombo;

            var result = _registry.SetBinding(settings, FeatureId.ZoomDesktop, ownCombo);

            Assert.That(result.BumpedFeatureId, Is.Null);
            Assert.That(settings.Features[FeatureId.PressWinKey].Enabled, Is.True);
        }

        [Test]
        public void SetBinding_ConflictDetectedRegardlessOfModifierOrderOrCase()
        {
            var settings = AppSettings.CreateDefault();
            var equivalentCombo = new KeyCombo(new[] { ModifierKey.RAlt }, "d"); // ZoomDesktop uses "D"

            var result = _registry.SetBinding(settings, FeatureId.PressWinKey, equivalentCombo);

            Assert.That(result.BumpedFeatureId, Is.EqualTo(FeatureId.ZoomDesktop));
        }

        [Test]
        public void SetBinding_InvalidCombo_ThrowsArgumentException()
        {
            var settings = AppSettings.CreateDefault();
            var invalidCombo = new KeyCombo(new ModifierKey[0], "D");

            Assert.Throws<System.ArgumentException>(() => _registry.SetBinding(settings, FeatureId.PressWinKey, invalidCombo));
        }
    }
}
