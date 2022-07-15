namespace CoffeeBackup.Lib.Handlers
{
    public interface IBackupHandler
    {
        Task TryCleanOldBackups(string[]? allFiles);
    }
}