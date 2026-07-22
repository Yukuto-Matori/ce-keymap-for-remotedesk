using System.Collections.Generic;
using CeKeymap.Core.Models;
using NUnit.Framework;

namespace CeKeymap.Core.Tests.Models
{
    public class KeyComboTests
    {
        [Test]
        public void Equals_SameModifiersAndKey_ReturnsTrue()
        {
            var a = new KeyCombo(new[] { ModifierKey.RAlt, ModifierKey.RShift }, "D");
            var b = new KeyCombo(new[] { ModifierKey.RAlt, ModifierKey.RShift }, "D");

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_ModifierOrderDiffers_ReturnsTrue()
        {
            var a = new KeyCombo(new[] { ModifierKey.RAlt, ModifierKey.RShift }, null);
            var b = new KeyCombo(new[] { ModifierKey.RShift, ModifierKey.RAlt }, null);

            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void Equals_MainKeyCaseDiffers_ReturnsTrue()
        {
            var a = new KeyCombo(new[] { ModifierKey.RAlt }, "d");
            var b = new KeyCombo(new[] { ModifierKey.RAlt }, "D");

            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void Equals_DuplicateModifiersInInput_AreDeduplicated()
        {
            var a = new KeyCombo(new[] { ModifierKey.RAlt, ModifierKey.RAlt }, "D");
            var b = new KeyCombo(new[] { ModifierKey.RAlt }, "D");

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.Modifiers.Count, Is.EqualTo(1));
        }

        [Test]
        public void Equals_DifferentMainKey_ReturnsFalse()
        {
            var a = new KeyCombo(new[] { ModifierKey.RAlt }, "D");
            var b = new KeyCombo(new[] { ModifierKey.RAlt }, "M");

            Assert.That(a, Is.Not.EqualTo(b));
        }

        [Test]
        public void Equals_DifferentModifierSet_ReturnsFalse()
        {
            var a = new KeyCombo(new[] { ModifierKey.RAlt }, "D");
            var b = new KeyCombo(new[] { ModifierKey.LAlt }, "D");

            Assert.That(a, Is.Not.EqualTo(b));
        }

        [Test]
        public void Equals_NullMainKeyOnBothSides_ReturnsTrue()
        {
            var a = new KeyCombo(new[] { ModifierKey.RAlt, ModifierKey.RShift }, null);
            var b = new KeyCombo(new[] { ModifierKey.RAlt, ModifierKey.RShift }, null);

            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void Constructor_NullModifierList_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => new KeyCombo(null, "D"));
        }

        [Test]
        public void Modifiers_ExposedAsReadOnlySet()
        {
            var combo = new KeyCombo(new List<ModifierKey> { ModifierKey.RAlt, ModifierKey.RShift }, "D");

            Assert.That(combo.Modifiers, Is.EquivalentTo(new[] { ModifierKey.RAlt, ModifierKey.RShift }));
        }
    }
}
