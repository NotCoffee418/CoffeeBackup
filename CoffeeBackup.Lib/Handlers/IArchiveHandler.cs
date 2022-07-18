namespace CoffeeBackup.Lib.Handlers
{
    public interface IArchiveHandler
    {
        Task<string> GenerateBackupAsync(string backupSourceDir, BackupReport workingReport, string? specificArchivePath = null);
    }
}