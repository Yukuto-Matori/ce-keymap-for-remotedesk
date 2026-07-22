using System;
using System.Collections.Generic;
using System.Linq;

namespace CeKeymap.Core.Models
{
    public sealed class KeyCombo : IEquatable<KeyCombo>
    {
        private readonly HashSet<ModifierKey> _modifiers;

        public KeyCombo(IEnumerable<ModifierKey> modifiers, string mainKey)
        {
            if (modifiers == null) throw new ArgumentNullException(nameof(modifiers));

            _modifiers = new HashSet<ModifierKey>(modifiers);
            MainKey = mainKey;
        }

        public IReadOnlyCollection<ModifierKey> Modifiers => _modifiers;

        public string MainKey { get; }

        public bool Equals(KeyCombo other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return _modifiers.SetEquals(other._modifiers)
                && string.Equals(NormalizeMainKey(MainKey), NormalizeMainKey(other.MainKey), StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) => Equals(obj as KeyCombo);

        public override int GetHashCode()
        {
            unchecked
            {
                var modifiersHash = Modifiers
                    .OrderBy(m => m)
                    .Aggregate(17, (hash, modifier) => hash * 31 + modifier.GetHashCode());

                var mainKeyHash = NormalizeMainKey(MainKey)?.ToUpperInvariant().GetHashCode() ?? 0;

                return modifiersHash * 31 + mainKeyHash;
            }
        }

        private static string NormalizeMainKey(string mainKey) =>
            string.IsNullOrEmpty(mainKey) ? null : mainKey;
    }
}
