using System;
using System.IO;

namespace CeKeymap.App.Infrastructure
{
    internal sealed class FileLogger
    {
        private readonly string _logFilePath;
        private readonly object _syncRoot = new object();

        public FileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public string FilePath => _logFilePath;

        public void Log(string message)
        {
            WriteLine($"[INFO] {message}");
        }

        public void LogError(string message, Exception exception = null)
        {
            var line = exception == null
                ? $"[ERROR] {message}"
                : $"[ERROR] {message}{Environment.NewLine}{exception}";

            WriteLine(line);
        }

        private void WriteLine(string line)
        {
            var entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {line}{Environment.NewLine}";

            lock (_syncRoot)
            {
                try
                {
                    File.AppendAllText(_logFilePath, entry);
                }
                catch (IOException)
                {
                    // Best-effort logging: a locked/unwritable log file must not crash the app.
                }
            }
        }
    }
}
