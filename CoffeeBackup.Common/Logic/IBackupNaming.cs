namespace CoffeeBackup.Common.Logic
{
    public interface IBackupNaming
    {
        DateTime? ExtractTimeFromBackupFileName(string filePathOrName);
        string GenerateBackupFileName(string? prefix = null);
    }
}