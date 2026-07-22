using CeKeymap.Core.Models;

namespace CeKeymap.Core.Keybinding
{
    public sealed class SetBindingResult
    {
        public SetBindingResult(FeatureId? bumpedFeatureId)
        {
            BumpedFeatureId = bumpedFeatureId;
        }

        /// <summary>
        /// The feature that lost its key binding (disabled, binding cleared) because the
        /// newly assigned combo was taken from it, or null if no conflict occurred.
        /// </summary>
        public FeatureId? BumpedFeatureId { get; }
    }
}
