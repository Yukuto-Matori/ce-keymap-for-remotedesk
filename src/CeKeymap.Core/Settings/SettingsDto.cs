using Newtonsoft.Json;

namespace CeKeymap.Core.Settings
{
    internal sealed class SettingsDto
    {
        [JsonProperty("Ver")]
        public int? Ver { get; set; }

        [JsonProperty("AppWindowSwitch")]
        public FeatureBindingDto AppWindowSwitch { get; set; }

        [JsonProperty("ZoomDesktop")]
        public FeatureBindingDto ZoomDesktop { get; set; }

        [JsonProperty("ZoomMobile")]
        public FeatureBindingDto ZoomMobile { get; set; }

        [JsonProperty("PressWinKey")]
        public FeatureBindingDto PressWinKey { get; set; }

        [JsonProperty("AutoStart")]
        public bool? AutoStart { get; set; }
    }

    internal sealed class FeatureBindingDto
    {
        [JsonProperty("Enabled")]
        public bool? Enabled { get; set; }

        [JsonProperty("Modifiers")]
        public string[] Modifiers { get; set; }

        [JsonProperty("Key")]
        public string Key { get; set; }

        [JsonProperty("ZoomPercent", NullValueHandling = NullValueHandling.Ignore)]
        public int? ZoomPercent { get; set; }
    }
}
