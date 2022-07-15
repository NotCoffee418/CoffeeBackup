using CoffeeBackup.Lib.Handlers;

namespace CoffeeBackup.Service.Workers;

public class BackupWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IStorageProvider _storageProvider;
    private readonly IBackupNaming _naming;
    private readonly IConfigAccess _configAccess;
    private readonly IArchiveHandler _archiveHandler;

    public BackupWorker(
        ILogger logger,
        IStorageProvider storageProvider,
        IBackupNaming naming,
        IConfigAccess configAccess,
        IArchiveHandler archiveHandler)
    {
        _logger = logger;
        _storageProvider = storageProvider;
        _naming = naming;
        _configAccess = configAccess;
        _archiveHandler = archiveHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.Information("Worker running at: {time}", DateTimeOffset.Now);
            try
            {
                await CheckAndTryBackupAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Backup attempt failed! Trying again in an hour... Error: {err}", ex.Message);                
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    /// <summary>
    /// Checks if we need to create a backup and do so if needed.
    /// </summary>
    /// <returns></returns>
    private async Task CheckAndTryBackupAsync()
    {
        _logger.Verbose("Get existing files in storage");
        string[] allFiles = await _storageProvider.ListFilesAsync();

        // Try cleanup old backups if defined in Service
        int? cleanupAfterDays = _configAccess.CleanupAfterDays();
        if (cleanupAfterDays is not null)
        {
            _logger.Verbose("Cleaning old backups...");
            DateTime oldBackupTimeBefore = DateTime.UtcNow.AddDays(cleanupAfterDays.Value);// * -1);
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



        // Determine previous backup time
        _logger.Verbose("Checking to see if new backup is needed...");
        DateTime? lastBackupTime = allFiles
            .Select(x => _naming.ExtractTimeFromBackupFileName(x))
            .Where(x => x is not null)
            .Select(x => x.Value)
            .OrderByDescending(x => x)
            .FirstOrDefault();
        _logger.Verbose("Previous backup was at {time}", lastBackupTime == null ? "never" : lastBackupTime);

        // Determine if we need to do a new backup
        DateTime newBackupRequiredAfter = lastBackupTime == null ? DateTime.MinValue :
            lastBackupTime.Value.AddDays(_configAccess.GetBackupIntervalDays());
        bool needBackup = DateTime.UtcNow > newBackupRequiredAfter;
        _logger.Verbose("Need backup? {need}", needBackup);
        if (!needBackup) return;

        // Do the backup
        string? archivePath = null;
        try
        {
            // Generate archive
            _logger.Verbose("Starting backup archive generation...");
            archivePath = await _archiveHandler.GenerateBackupAsync("/backup/");
            _logger.Verbose("Backup archive generated at {path}", archivePath);

            // Upload archive
            string desiredFileName = _naming.GenerateBackupFileName();
            await _storageProvider.UploadBackupAsync(archivePath, desiredFileName);
            _logger.Information("Backup complete");
        }
        catch (Exception ex)
        {
            _logger.Error("Backup failed with error {exMsg}", ex.Message);
        }
        finally // Cleanup
        {
            if (archivePath is not null && File.Exists(archivePath))
            {
                _logger.Verbose("Local archive deleted: {path}", archivePath);
                File.Delete(archivePath);
            }
        }
    }
}