using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CeKeymap.App.Forms
{
    /// <summary>
    /// Dropdown-based zoom percent editor. Free-text percent entry let users pick a value that
    /// didn't correspond to any actual Windows display-scaling option, so the DPI feature would
    /// silently snap to the nearest real option instead of the requested one; this form limits
    /// the choice to Windows' own standard scaling steps up front.
    /// </summary>
    internal sealed class ZoomPercentEditForm : Form
    {
        private static readonly int[] StandardPercentOptions = { 100, 125, 150, 175, 200, 225, 250, 300 };

        private readonly ComboBox _percentCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

        public int SelectedPercent { get; private set; }

        public ZoomPercentEditForm(int currentPercent)
        {
            Text = "拡大率の指定";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(240, 110);

            var label = new Label { Text = "拡大率", Location = new Point(12, 15), AutoSize = true };
            _percentCombo.Location = new Point(80, 12);
            _percentCombo.Width = 140;
            _percentCombo.Items.AddRange(StandardPercentOptions.Select(p => (object)$"{p}%").ToArray());

            var closest = StandardPercentOptions.OrderBy(p => Math.Abs(p - currentPercent)).First();
            _percentCombo.SelectedItem = $"{closest}%";

            var okButton = new Button { Text = "OK", Location = new Point(60, 65) };
            okButton.Click += (s, e) =>
            {
                var text = (string)_percentCombo.SelectedItem;
                SelectedPercent = int.Parse(text.TrimEnd('%'));
                DialogResult = DialogResult.OK;
                Close();
            };

            var cancelButton = new Button { Text = "キャンセル", DialogResult = DialogResult.Cancel, Location = new Point(145, 65) };

            Controls.AddRange(new Control[] { label, _percentCombo, okButton, cancelButton });
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }
    }
}
