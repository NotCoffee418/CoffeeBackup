namespace CoffeeBackup.Common.Data;

public class Enums
{
    public enum Scope
    {
        Undefined = 0,
        // Every injection new instance
        PerDependency = 1,
        // one instance is returned from all requests in the root and all nested scopes
        Single = 2,
        // One lifetime instance
        Lifetime = 3,
    }

    public enum BackupStatus
    {
        Undefined = 0,
        // Backup succeeded
        Success = 1,
        // Backup is running after a long period, possible hang
        Inconclusive = 2,
        // Backup has failed
        Failed = 3,
    }
}
