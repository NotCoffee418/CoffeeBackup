namespace CoffeeBackup.Lib.Handlers
{
    public interface IArchiveHandler
    {
        Task<string> GenerateBackupAsync(string backupSourceDir, string? specificArchivePath = null);
    }
}