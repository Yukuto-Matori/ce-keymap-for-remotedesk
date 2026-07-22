using System;

namespace CeKeymap.Core.Settings
{
    public sealed class SettingsCorruptedException : Exception
    {
        public SettingsCorruptedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SettingsCorruptedException(string message)
            : base(message)
        {
        }
    }
}
