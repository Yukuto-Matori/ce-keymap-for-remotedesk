using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CeKeymap.Core.Localization;
using Newtonsoft.Json;

namespace CeKeymap.App.Infrastructure
{
    /// <summary>
    /// Loads the locale JSON matching the current system UI culture from the "locale" folder
    /// (next to the exe), with English as the fallback for both locale selection and per-key
    /// lookups within <see cref="LocalizationCatalog"/>.
    /// </summary>
    internal sealed class LocaleFileLoader
    {
        private const string FallbackLocale = "en";

        private readonly string _localeDirectory;
        private readonly FileLogger _logger;
        private readonly LocaleResolver _resolver = new LocaleResolver();

        public LocaleFileLoader(string localeDirectory, FileLogger logger)
        {
            _localeDirectory = localeDirectory;
            _logger = logger;
        }

        public LocalizationCatalog Load()
        {
            if (!Directory.Exists(_localeDirectory))
            {
                _logger.LogError($"Locale directory not found: {_localeDirectory}");
                return new LocalizationCatalog(new Dictionary<string, string>(), new Dictionary<string, string>());
            }

            var availableLocales = Directory.GetFiles(_localeDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();

            var requestedLocale = CultureInfo.CurrentUICulture.Name;
            var selectedLocale = _resolver.Resolve(requestedLocale, availableLocales);

            _logger.Log($"Locale resolution: requested='{requestedLocale}', available=[{string.Join(",", availableLocales)}], selected='{selectedLocale ?? "(none)"}'.");

            var primary = selectedLocale != null ? LoadDictionary(selectedLocale) : new Dictionary<string, string>();
            var fallback = string.Equals(selectedLocale, FallbackLocale)
                ? primary
                : LoadDictionary(FallbackLocale);

            return new LocalizationCatalog(primary, fallback);
        }

        private Dictionary<string, string> LoadDictionary(string locale)
        {
            var path = Path.Combine(_localeDirectory, locale + ".json");
            if (!File.Exists(path)) return new Dictionary<string, string>();

            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Failed to parse locale file '{path}'.", ex);
                return new Dictionary<string, string>();
            }
        }
    }
}
