namespace CeKeymap.Core.Models
{
    public sealed class FeatureBinding
    {
        public FeatureBinding(FeatureId featureId, bool enabled, KeyCombo keyCombo, int? zoomPercent = null)
        {
            FeatureId = featureId;
            Enabled = enabled;
            KeyCombo = keyCombo;
            ZoomPercent = zoomPercent;
        }

        public FeatureId FeatureId { get; }

        public bool Enabled { get; set; }

        public KeyCombo KeyCombo { get; set; }

        public int? ZoomPercent { get; set; }

        public FeatureBinding Clone() => new FeatureBinding(FeatureId, Enabled, KeyCombo, ZoomPercent);
    }
}
