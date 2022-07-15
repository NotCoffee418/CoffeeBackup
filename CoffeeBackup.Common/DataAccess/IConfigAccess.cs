namespace CoffeeBackup.Common.DataAccess
{
    public interface IConfigAccess
    {
        int GetBackupIntervalDays();
        string GetStorjAccessGrant();
        string GetStorjBackupBucket();
    }
}