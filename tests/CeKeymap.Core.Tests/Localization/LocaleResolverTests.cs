using System.Collections.Generic;
using CeKeymap.Core.Localization;
using NUnit.Framework;

namespace CeKeymap.Core.Tests.Localization
{
    public class LocaleResolverTests
    {
        private readonly LocaleResolver _resolver = new LocaleResolver();

        [Test]
        public void Resolve_ExactMatch_ReturnsExactLocale()
        {
            var result = _resolver.Resolve("ja", new[] { "en", "ja" });

            Assert.That(result, Is.EqualTo("ja"));
        }

        [Test]
        public void Resolve_ExactMatchIsCaseInsensitive()
        {
            var result = _resolver.Resolve("JA", new[] { "en", "ja" });

            Assert.That(result, Is.EqualTo("ja"));
        }

        [Test]
        public void Resolve_RegionSpecificRequest_FallsBackToLanguageOnlyMatch()
        {
            var result = _resolver.Resolve("ja-JP", new[] { "en", "ja" });

            Assert.That(result, Is.EqualTo("ja"));
        }

        [Test]
        public void Resolve_NoMatch_FallsBackToEnglish()
        {
            var result = _resolver.Resolve("fr-FR", new[] { "en", "ja" });

            Assert.That(result, Is.EqualTo("en"));
        }

        [Test]
        public void Resolve_NoMatchAndNoEnglishAvailable_ReturnsNull()
        {
            var result = _resolver.Resolve("fr-FR", new[] { "ja" });

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Resolve_NoLocalesAvailable_ReturnsNull()
        {
            var result = _resolver.Resolve("ja-JP", new string[0]);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Resolve_NullRequestedLocale_FallsBackToEnglish()
        {
            var result = _resolver.Resolve(null, new[] { "en", "ja" });

            Assert.That(result, Is.EqualTo("en"));
        }
    }
}
