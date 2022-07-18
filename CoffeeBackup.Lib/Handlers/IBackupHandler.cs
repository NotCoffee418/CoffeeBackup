namespace CoffeeBackup.Lib.Handlers
{
    public interface IBackupHandler
    {
        string GetHumanBackupSize(string archivePath);
        Task TryCleanOldBackups(string[]? allObjects);
    }
}