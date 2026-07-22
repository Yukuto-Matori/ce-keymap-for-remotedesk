using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CeKeymap.App.Infrastructure;
using CeKeymap.Core.Keybinding;
using CeKeymap.Core.Models;

namespace CeKeymap.App.Forms
{
    /// <summary>
    /// Dropdown-based keybinding editor (no key-capture input), per spec: modifier and main-key
    /// choices are made from combo boxes, and modifier-less/main-key-only combos are rejected.
    /// </summary>
    internal sealed class KeybindingEditForm : Form
    {
        private static string None => Loc.Get("keybindingForm.none");

        private readonly ComboBox _modifier1Combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _modifier2Combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _mainKeyCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _errorLabel = new Label { ForeColor = Color.Red, AutoSize = true };
        private readonly KeybindingRegistry _registry = new KeybindingRegistry();
        private readonly AppSettings _settings;
        private readonly FeatureId _featureId;

        public KeybindingEditForm(AppSettings settings, FeatureId featureId)
        {
            _settings = settings;
            _featureId = featureId;

            Text = Loc.Get("keybindingForm.title", Loc.FeatureLabel(featureId));
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(320, 190);

            BuildLayout();
            LoadCurrentBinding();
        }

        private void BuildLayout()
        {
            var modifierValues = new[] { None }
                .Concat(Enum.GetNames(typeof(ModifierKey)))
                .ToArray();
            var mainKeyValues = new[] { None }
                .Concat(Enumerable.Range('A', 26).Select(c => ((char)c).ToString()))
                .ToArray();

            _modifier1Combo.Items.AddRange(modifierValues);
            _modifier2Combo.Items.AddRange(modifierValues);
            _mainKeyCombo.Items.AddRange(mainKeyValues);

            var label1 = new Label { Text = Loc.Get("keybindingForm.modifier1"), Location = new Point(12, 15), AutoSize = true };
            _modifier1Combo.Location = new Point(120, 12);
            _modifier1Combo.Width = 180;

            var label2 = new Label { Text = Loc.Get("keybindingForm.modifier2"), Location = new Point(12, 45), AutoSize = true };
            _modifier2Combo.Location = new Point(120, 42);
            _modifier2Combo.Width = 180;

            var label3 = new Label { Text = Loc.Get("keybindingForm.mainKey"), Location = new Point(12, 75), AutoSize = true };
            _mainKeyCombo.Location = new Point(120, 72);
            _mainKeyCombo.Width = 180;

            _errorLabel.Location = new Point(12, 105);
            _errorLabel.MaximumSize = new Size(296, 40);

            var okButton = new Button { Text = Loc.Get("common.ok"), DialogResult = DialogResult.None, Location = new Point(140, 150) };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button { Text = Loc.Get("common.cancel"), DialogResult = DialogResult.Cancel, Location = new Point(225, 150) };

            Controls.AddRange(new Control[]
            {
                label1, _modifier1Combo,
                label2, _modifier2Combo,
                label3, _mainKeyCombo,
                _errorLabel,
                okButton, cancelButton,
            });

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        private void LoadCurrentBinding()
        {
            var combo = _settings.Features[_featureId].KeyCombo;
            var modifiers = combo?.Modifiers.ToArray() ?? Array.Empty<ModifierKey>();

            _modifier1Combo.SelectedItem = modifiers.Length > 0 ? modifiers[0].ToString() : None;
            _modifier2Combo.SelectedItem = modifiers.Length > 1 ? modifiers[1].ToString() : None;
            _mainKeyCombo.SelectedItem = combo?.MainKey ?? None;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var modifiers = new[] { _modifier1Combo.SelectedItem as string, _modifier2Combo.SelectedItem as string }
                .Where(m => m != null && m != None)
                .Select(m => (ModifierKey)Enum.Parse(typeof(ModifierKey), m))
                .Distinct()
                .ToArray();

            var mainKeySelection = _mainKeyCombo.SelectedItem as string;
            var mainKey = mainKeySelection == None ? null : mainKeySelection;

            var newCombo = new KeyCombo(modifiers, mainKey);

            if (!_registry.Validate(newCombo))
            {
                _errorLabel.Text = Loc.Get("keybindingForm.validationError");
                return;
            }

            var result = _registry.SetBinding(_settings, _featureId, newCombo);
            if (result.BumpedFeatureId.HasValue)
            {
                MessageBox.Show(
                    Loc.Get("tray.dialog.keybindingBumped", Loc.FeatureLabel(result.BumpedFeatureId.Value)),
                    "CeKeymap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
