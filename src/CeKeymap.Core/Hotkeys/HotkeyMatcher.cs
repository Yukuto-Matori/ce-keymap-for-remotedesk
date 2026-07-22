using System.Collections.Generic;
using System.Linq;
using CeKeymap.Core.Models;

namespace CeKeymap.Core.Hotkeys
{
    /// <summary>
    /// Determines which enabled feature (if any) the currently pressed keys trigger.
    /// A match requires the pressed modifier set to equal a registered combo exactly -
    /// extra modifiers held down never match, to avoid accidental triggers.
    /// </summary>
    public sealed class HotkeyMatcher
    {
        public FeatureId? Match(IEnumerable<ModifierKey> pressedModifiers, string pressedMainKey, AppSettings settings)
        {
            var pressedCombo = new KeyCombo(pressedModifiers, pressedMainKey);

            var matched = settings.Features.Values
                .Where(binding => binding.Enabled && binding.KeyCombo != null && binding.KeyCombo.Equals(pressedCombo))
                .FirstOrDefault();

            return matched?.FeatureId;
        }
    }
}
