using Microsoft.Win32;

namespace CeKeymap.App.Infrastructure
{
    internal sealed class AutoStartService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "CeKeymapForRemotedesk";

        private readonly string _executablePath;

        public AutoStartService(string executablePath)
        {
            _executablePath = executablePath;
        }

        public void SetEnabled(bool enabled)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true))
            {
                if (key == null) return;

                if (enabled)
                {
                    key.SetValue(ValueName, $"\"{_executablePath}\"");
                }
                else
                {
                    key.DeleteValue(ValueName, throwOnMissingValue: false);
                }
            }
        }
    }
}
