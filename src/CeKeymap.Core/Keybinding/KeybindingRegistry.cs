using System;
using System.Linq;
using CeKeymap.Core.Models;

namespace CeKeymap.Core.Keybinding
{
    /// <summary>
    /// Validates key combinations and resolves cross-feature conflicts with a
    /// last-write-wins rule: assigning a combo already held by another feature
    /// disables that other feature and clears its binding.
    /// </summary>
    public sealed class KeybindingRegistry
    {
        public bool Validate(KeyCombo combo)
        {
            if (combo == null) return false;
            if (combo.Modifiers.Count == 0) return false;
            if (combo.MainKey == null && combo.Modifiers.Count < 2) return false;

            return true;
        }

        public SetBindingResult SetBinding(AppSettings settings, FeatureId featureId, KeyCombo newCombo)
        {
            if (!Validate(newCombo))
            {
                throw new ArgumentException("Key combination requires at least one modifier and either a main key or a second modifier.", nameof(newCombo));
            }

            var previousHolder = settings.Features.Values
                .Where(binding => binding.FeatureId != featureId && binding.KeyCombo != null && binding.KeyCombo.Equals(newCombo))
                .FirstOrDefault();

            if (previousHolder != null)
            {
                previousHolder.Enabled = false;
                previousHolder.KeyCombo = null;
            }

            var target = settings.Features[featureId];
            target.KeyCombo = newCombo;
            target.Enabled = true;

            return new SetBindingResult(previousHolder?.FeatureId);
        }
    }
}
