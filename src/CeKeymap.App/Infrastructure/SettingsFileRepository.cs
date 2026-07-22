using System.IO;
using System.Windows.Forms;
using CeKeymap.Core.Models;
using CeKeymap.Core.Settings;

namespace CeKeymap.App.Infrastructure
{
    internal sealed class SettingsFileRepository
    {
        private readonly string _filePath;
        private readonly SettingsSerializer _serializer = new SettingsSerializer();
        private readonly SettingsMigrator _migrator = new SettingsMigrator();
        private readonly FileLogger _logger;

        public SettingsFileRepository(string filePath, FileLogger logger)
        {
            _filePath = filePath;
            _logger = logger;
        }

        public AppSettings Load()
        {
            if (!File.Exists(_filePath))
            {
                var defaults = AppSettings.CreateDefault();
                Save(defaults);
                return defaults;
            }

            AppSettings parsed;
            try
            {
                var json = File.ReadAllText(_filePath);
                parsed = _serializer.Deserialize(json);
            }
            catch (SettingsCorruptedException ex)
            {
                _logger.LogError("settings.json is corrupted; restoring defaults.", ex);
                MessageBox.Show(
                    Loc.Get("settings.dialog.corrupted"),
                    "CeKeymap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                var defaults = AppSettings.CreateDefault();
                Save(defaults);
                return defaults;
            }

            var result = _migrator.Reconcile(parsed);
            if (result.AppliedDefaultsDueToVersionMismatch)
            {
                _logger.Log("settings.json version mismatch; some fields were reset to defaults.");
                MessageBox.Show(
                    Loc.Get("settings.dialog.versionMismatch"),
                    "CeKeymap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            Save(result.Settings);
            return result.Settings;
        }

        public void Save(AppSettings settings)
        {
            var json = _serializer.Serialize(settings);
            File.WriteAllText(_filePath, json);
        }
    }
}
