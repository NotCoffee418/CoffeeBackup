namespace CoffeeBackup.Lib.Handlers;

public class BackupHandler : IBackupHandler
{
    private IConfigAccess _configAccess;
    private IStorageProvider _storageProvider;
    private ILogger _logger;
    private IBackupNaming _naming;

    public BackupHandler(
        IConfigAccess configAccess,
        IStorageProvider storageProvider,
        ILogger logger,
        IBackupNaming naming)
    {
        _configAccess = configAccess;
        _storageProvider = storageProvider;
        _logger = logger;
        _naming = naming;
    }

    public async Task TryCleanOldBackups(string[]? allFiles)
    {
        if (allFiles is null)
        {
            allFiles = await _storageProvider.ListFilesAsync();
        }

        // Try cleanup old backups if defined in Service
        int? cleanupAfterDays = _configAccess.CleanupAfterDays();
        if (cleanupAfterDays is not null)
        {
            _logger.Verbose("Cleaning old backups...");
            DateTime oldBackupTimeBefore = DateTime.UtcNow.AddDays(cleanupAfterDays.Value * -1);
            string[] filesToClean = allFiles
                .Select(x => (_naming.ExtractTimeFromBackupFileName(x), x))
                .Where(x => x.Item1 < oldBackupTimeBefore)
                .Select(x => x.Item2)
                .ToArray();
            foreach (string file in filesToClean)
            {
                _logger.Information("Removing old backup: {name}");
                await _storageProvider.RemoveFileAsync(file);
            }
            _logger.Verbose("Cleaning old backups complete.");
        }
    }
}
