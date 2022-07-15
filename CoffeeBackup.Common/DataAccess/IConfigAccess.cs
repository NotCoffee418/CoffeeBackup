namespace CoffeeBackup.Common.DataAccess
{
    public interface IConfigAccess
    {
        string GetStorjAccessGrant();
        string GetStorjBackupBucket();
    }
}