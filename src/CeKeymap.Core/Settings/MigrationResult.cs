using CeKeymap.Core.Models;

namespace CeKeymap.Core.Settings
{
    public sealed class MigrationResult
    {
        public MigrationResult(AppSettings settings, bool appliedDefaultsDueToVersionMismatch)
        {
            Settings = settings;
            AppliedDefaultsDueToVersionMismatch = appliedDefaultsDueToVersionMismatch;
        }

        public AppSettings Settings { get; }

        public bool AppliedDefaultsDueToVersionMismatch { get; }
    }
}
