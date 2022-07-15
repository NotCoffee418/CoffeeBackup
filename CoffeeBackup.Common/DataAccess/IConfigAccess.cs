namespace CoffeeBackup.Common.DataAccess
{
    public interface IConfigAccess
    {
        int? CleanupAfterDays();
        int GetBackupIntervalDays();
        string GetStorjAccessGrant();
        string GetStorjBackupBucket();
    }
}