namespace CoffeeBackup.Common.DataAccess;

public class ConfigAccess : IConfigAccess
{
    private IConfiguration _configuration;
    private ILogger _logger;

    public ConfigAccess(
        IConfiguration configuration,
        ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GetStorjAccessGrant()
        => ReadString("StorjAccessGrant", false);

    public string GetStorjBackupBucket()
        => ReadString("StorjBackupBucket", false);

    public int GetBackupIntervalDays()
    {
        int result = ReadInt("BackupIntervalDays", false) ?? 0;
        if (result < 1)
            throw new Exception($"Configuration key 'BackupIntervalDays' is invalid");
        return result;
    }

    public int? CleanupAfterDays()
        => ReadInt("CleanupAfterDays", true);


    private string ReadString(string key, bool allowNullOrEmpty)
    {
        string? result = ReadValue<string>(key, allowNullOrEmpty);
        if (!allowNullOrEmpty && string.IsNullOrEmpty(result))
            throw new Exception($"Configuration key '{key}' is missing or empty");
        return result ?? "impossible";
    }

    private int? ReadInt(string key, bool allowNull = false)
    {
        int? value = ReadValue<int?>(key, allowNull);
        if (!allowNull && value is null)
            throw new Exception($"Config value {key} is null");
        return value.HasValue ? value.Value : null;
    }

    private T? ReadValue<T>(string key, bool allowNull)
    {
        try
        {
            T? value = _configuration.GetValue<T>(key.ToString());
            if (value is null && !allowNull)
                throw new Exception($"Config value {key} is null");
            return value;
        }
        catch
        {
            _logger.Fatal("Failed to read the required config value for '{key}'. " +
                "Please ensure the config file or environment variables are set up correctly.", key);
            throw;
        }
    }
}
