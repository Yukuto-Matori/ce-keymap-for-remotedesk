using System.Collections.Generic;

namespace CeKeymap.Core.Localization
{
    /// <summary>
    /// Resolves a UI string key against a primary (selected-locale) dictionary, falling back
    /// to a secondary dictionary (typically English) and finally to the key itself, so a
    /// missing translation is visible rather than causing a crash or blank label.
    /// </summary>
    public sealed class LocalizationCatalog
    {
        private readonly IDictionary<string, string> _primary;
        private readonly IDictionary<string, string> _fallback;

        public LocalizationCatalog(IDictionary<string, string> primary, IDictionary<string, string> fallback)
        {
            _primary = primary ?? new Dictionary<string, string>();
            _fallback = fallback ?? new Dictionary<string, string>();
        }

        public string Get(string key)
        {
            if (_primary.TryGetValue(key, out var primaryValue)) return primaryValue;
            if (_fallback.TryGetValue(key, out var fallbackValue)) return fallbackValue;
            return key;
        }

        public string Get(string key, params object[] args) => string.Format(Get(key), args);
    }
}
