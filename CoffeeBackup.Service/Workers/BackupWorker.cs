using CoffeeBackup.Lib.Handlers;

namespace CoffeeBackup.Service.Workers;

public class BackupWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IStorageProvider _storageProvider;
    private readonly IBackupNaming _naming;
    private readonly IArchiveHandler _archiveHandler;
    private readonly IBackupHandler _backupHandler;
    private readonly IConfiguration _configuration;

    public BackupWorker(
        ILogger logger,
        IStorageProvider storageProvider,
        IBackupNaming naming,
        IArchiveHandler archiveHandler,
        IBackupHandler backupHandler,
        IConfiguration configuration)
    {
        _logger = logger;
        _storageProvider = storageProvider;
        _naming = naming;
        _archiveHandler = archiveHandler;
        _backupHandler = backupHandler;
        _configuration = configuration;
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
        _logger.Verbose("Starting new backup check...");
        string[] allObjects = await _storageProvider.ListFilesAsync();

        // Try cleanup old backups if defined in Service
        _logger.Verbose("Attempting to clean old backups if configured");
        await _backupHandler.TryCleanOldBackups(allObjects);            

        // Determine previous backup time
        _logger.Verbose("Checking to see if new backup is needed...");
        DateTime? lastBackupTime = allObjects
            .Select(x => _naming.ExtractTimeFromBackupFileName(x))
            .Where(x => x is not null)
            .Select(x => x.Value)
            .OrderByDescending(x => x)
            .FirstOrDefault();
        _logger.Verbose("Previous backup was at {time}", lastBackupTime == null ? "never" : lastBackupTime);

        // Determine if we need to do a new backup
        int intervalDays = _configuration.GetRequiredSection("BackupSettings:BackupIntervalDays").Get<int>();
        DateTime newBackupRequiredAfter = lastBackupTime == null ? DateTime.MinValue :
            lastBackupTime.Value.AddDays(intervalDays);
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
            _logger.Error("Backup failed: {exMsg}", ex.Message);
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