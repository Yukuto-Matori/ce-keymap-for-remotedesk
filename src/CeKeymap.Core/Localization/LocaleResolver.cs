using System;
using System.Collections.Generic;
using System.Linq;

namespace CeKeymap.Core.Localization
{
    /// <summary>
    /// Picks the best available locale for a requested one: exact match, then a
    /// language-only match (e.g. "ja-JP" -> "ja"), then falls back to English.
    /// </summary>
    public sealed class LocaleResolver
    {
        private const string FallbackLocale = "en";

        public string Resolve(string requestedLocale, IEnumerable<string> availableLocales)
        {
            var available = availableLocales.ToList();

            var exactMatch = available.FirstOrDefault(l => string.Equals(l, requestedLocale, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null) return exactMatch;

            var language = requestedLocale?.Split('-').FirstOrDefault();
            if (!string.IsNullOrEmpty(language))
            {
                var languageMatch = available.FirstOrDefault(l => string.Equals(l, language, StringComparison.OrdinalIgnoreCase));
                if (languageMatch != null) return languageMatch;
            }

            return available.FirstOrDefault(l => string.Equals(l, FallbackLocale, StringComparison.OrdinalIgnoreCase));
        }
    }
}
