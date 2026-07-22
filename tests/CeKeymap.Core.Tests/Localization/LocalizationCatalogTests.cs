using System.Collections.Generic;
using CeKeymap.Core.Localization;
using NUnit.Framework;

namespace CeKeymap.Core.Tests.Localization
{
    public class LocalizationCatalogTests
    {
        [Test]
        public void Get_KeyInPrimary_ReturnsPrimaryValue()
        {
            var primary = new Dictionary<string, string> { ["greeting"] = "こんにちは" };
            var fallback = new Dictionary<string, string> { ["greeting"] = "Hello" };
            var catalog = new LocalizationCatalog(primary, fallback);

            Assert.That(catalog.Get("greeting"), Is.EqualTo("こんにちは"));
        }

        [Test]
        public void Get_KeyMissingFromPrimary_FallsBackToFallbackDictionary()
        {
            var primary = new Dictionary<string, string>();
            var fallback = new Dictionary<string, string> { ["greeting"] = "Hello" };
            var catalog = new LocalizationCatalog(primary, fallback);

            Assert.That(catalog.Get("greeting"), Is.EqualTo("Hello"));
        }

        [Test]
        public void Get_KeyMissingFromBoth_ReturnsKeyItself()
        {
            var catalog = new LocalizationCatalog(new Dictionary<string, string>(), new Dictionary<string, string>());

            Assert.That(catalog.Get("unknown.key"), Is.EqualTo("unknown.key"));
        }

        [Test]
        public void Get_NullFallbackDictionary_StillWorksForPrimaryHits()
        {
            var primary = new Dictionary<string, string> { ["greeting"] = "こんにちは" };
            var catalog = new LocalizationCatalog(primary, null);

            Assert.That(catalog.Get("greeting"), Is.EqualTo("こんにちは"));
        }

        [Test]
        public void Get_NullFallbackDictionary_MissingKeyReturnsKeyItself()
        {
            var catalog = new LocalizationCatalog(new Dictionary<string, string>(), null);

            Assert.That(catalog.Get("missing"), Is.EqualTo("missing"));
        }

        [Test]
        public void GetFormatted_SubstitutesArgsIntoValue()
        {
            var primary = new Dictionary<string, string> { ["bumped"] = "{0} was turned off" };
            var catalog = new LocalizationCatalog(primary, null);

            Assert.That(catalog.Get("bumped", "ZoomDesktop"), Is.EqualTo("ZoomDesktop was turned off"));
        }
    }
}
