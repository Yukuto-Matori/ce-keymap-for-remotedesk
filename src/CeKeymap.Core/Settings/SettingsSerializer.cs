using System;
using System.Collections.Generic;
using System.Linq;
using CeKeymap.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CeKeymap.Core.Settings
{
    public sealed class SettingsSerializer
    {
        public string Serialize(AppSettings settings)
        {
            var dto = new SettingsDto
            {
                Ver = settings.Ver,
                AutoStart = settings.AutoStart,
                AppWindowSwitch = ToDto(settings, FeatureId.AppWindowSwitch),
                ZoomDesktop = ToDto(settings, FeatureId.ZoomDesktop),
                ZoomMobile = ToDto(settings, FeatureId.ZoomMobile),
                PressWinKey = ToDto(settings, FeatureId.PressWinKey),
            };

            return JsonConvert.SerializeObject(dto, Formatting.Indented);
        }

        /// <summary>
        /// Missing/absent fields (e.g. an older-schema file) are simply omitted from the
        /// result rather than defaulted; reconciling with defaults is <see cref="SettingsMigrator"/>'s job.
        /// </summary>
        public AppSettings Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new SettingsCorruptedException("settings.json is empty.");
            }

            SettingsDto dto;
            try
            {
                var token = JToken.Parse(json);
                if (token.Type != JTokenType.Object)
                {
                    throw new SettingsCorruptedException("settings.json does not contain a JSON object.");
                }

                dto = token.ToObject<SettingsDto>();
            }
            catch (JsonException ex)
            {
                throw new SettingsCorruptedException("settings.json could not be parsed as JSON.", ex);
            }

            var settings = new AppSettings
            {
                Ver = dto.Ver ?? 0,
                AutoStart = dto.AutoStart ?? false,
                Features = new Dictionary<FeatureId, FeatureBinding>(),
            };

            AddIfPresent(settings, FeatureId.AppWindowSwitch, dto.AppWindowSwitch);
            AddIfPresent(settings, FeatureId.ZoomDesktop, dto.ZoomDesktop);
            AddIfPresent(settings, FeatureId.ZoomMobile, dto.ZoomMobile);
            AddIfPresent(settings, FeatureId.PressWinKey, dto.PressWinKey);

            return settings;
        }

        private static void AddIfPresent(AppSettings settings, FeatureId featureId, FeatureBindingDto dto)
        {
            if (dto == null) return;

            var modifiers = (dto.Modifiers ?? Array.Empty<string>())
                .Select(ParseModifier)
                .Where(m => m.HasValue)
                .Select(m => m.Value);

            settings.Features[featureId] = new FeatureBinding(
                featureId,
                enabled: dto.Enabled ?? false,
                keyCombo: new KeyCombo(modifiers, dto.Key),
                zoomPercent: dto.ZoomPercent);
        }

        private static ModifierKey? ParseModifier(string value) =>
            Enum.TryParse(value, out ModifierKey modifier) ? modifier : (ModifierKey?)null;

        private static FeatureBindingDto ToDto(AppSettings settings, FeatureId featureId)
        {
            if (!settings.Features.TryGetValue(featureId, out var binding)) return null;

            return new FeatureBindingDto
            {
                Enabled = binding.Enabled,
                Modifiers = binding.KeyCombo.Modifiers.Select(m => m.ToString()).ToArray(),
                Key = binding.KeyCombo.MainKey,
                ZoomPercent = binding.ZoomPercent,
            };
        }
    }
}
