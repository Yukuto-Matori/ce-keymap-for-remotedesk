using CeKeymap.Core.Models;
using CeKeymap.Core.Settings;
using NUnit.Framework;

namespace CeKeymap.Core.Tests.Settings
{
    public class SettingsSerializerTests
    {
        private readonly SettingsSerializer _serializer = new SettingsSerializer();

        [Test]
        public void RoundTrip_DefaultSettings_PreservesAllValues()
        {
            var original = AppSettings.CreateDefault();

            var json = _serializer.Serialize(original);
            var restored = _serializer.Deserialize(json);

            Assert.That(restored.Ver, Is.EqualTo(original.Ver));
            Assert.That(restored.AutoStart, Is.EqualTo(original.AutoStart));
            Assert.That(restored.Features.Count, Is.EqualTo(original.Features.Count));

            foreach (var featureId in original.Features.Keys)
            {
                var expected = original.Features[featureId];
                var actual = restored.Features[featureId];

                Assert.That(actual.Enabled, Is.EqualTo(expected.Enabled), featureId.ToString());
                Assert.That(actual.KeyCombo, Is.EqualTo(expected.KeyCombo), featureId.ToString());
                Assert.That(actual.ZoomPercent, Is.EqualTo(expected.ZoomPercent), featureId.ToString());
            }
        }

        [Test]
        public void RoundTrip_ModifiedSettings_PreservesChanges()
        {
            var original = AppSettings.CreateDefault();
            original.AutoStart = true;
            original.Features[FeatureId.ZoomDesktop].ZoomPercent = 125;
            original.Features[FeatureId.PressWinKey].Enabled = false;

            var restored = _serializer.Deserialize(_serializer.Serialize(original));

            Assert.That(restored.AutoStart, Is.True);
            Assert.That(restored.Features[FeatureId.ZoomDesktop].ZoomPercent, Is.EqualTo(125));
            Assert.That(restored.Features[FeatureId.PressWinKey].Enabled, Is.False);
        }

        [Test]
        public void Deserialize_KnownFixedJson_MapsFieldsCorrectly()
        {
            const string json = @"{
                ""Ver"": 1,
                ""AppWindowSwitch"": { ""Enabled"": true, ""Modifiers"": [""RAlt"", ""RShift""], ""Key"": null },
                ""ZoomDesktop"": { ""Enabled"": true, ""Modifiers"": [""RAlt""], ""Key"": ""D"", ""ZoomPercent"": 100 },
                ""ZoomMobile"": { ""Enabled"": true, ""Modifiers"": [""RAlt""], ""Key"": ""M"", ""ZoomPercent"": 150 },
                ""PressWinKey"": { ""Enabled"": true, ""Modifiers"": [""RShift""], ""Key"": ""W"" },
                ""AutoStart"": false
            }";

            var settings = _serializer.Deserialize(json);

            var appWindowSwitch = settings.Features[FeatureId.AppWindowSwitch];
            Assert.That(appWindowSwitch.KeyCombo.Modifiers, Is.EquivalentTo(new[] { ModifierKey.RAlt, ModifierKey.RShift }));
            Assert.That(appWindowSwitch.KeyCombo.MainKey, Is.Null);

            var zoomDesktop = settings.Features[FeatureId.ZoomDesktop];
            Assert.That(zoomDesktop.ZoomPercent, Is.EqualTo(100));
        }

        [Test]
        public void Deserialize_MalformedJson_ThrowsSettingsCorruptedException()
        {
            const string malformed = "{ this is not valid json ";

            Assert.Throws<SettingsCorruptedException>(() => _serializer.Deserialize(malformed));
        }

        [Test]
        public void Deserialize_EmptyString_ThrowsSettingsCorruptedException()
        {
            Assert.Throws<SettingsCorruptedException>(() => _serializer.Deserialize(string.Empty));
        }

        [Test]
        public void Deserialize_JsonArrayInsteadOfObject_ThrowsSettingsCorruptedException()
        {
            Assert.Throws<SettingsCorruptedException>(() => _serializer.Deserialize("[1, 2, 3]"));
        }
    }
}
