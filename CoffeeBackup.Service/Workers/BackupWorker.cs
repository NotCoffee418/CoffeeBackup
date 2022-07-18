using static CoffeeBackup.Common.Data.Enums;

namespace CoffeeBackup.Service.Workers;

public class BackupWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IStorageProvider _storageProvider;
    private readonly IBackupNaming _naming;
    private readonly IArchiveHandler _archiveHandler;
    private readonly IBackupHandler _backupHandler;
    private readonly IConfiguration _configuration;
    private readonly INotificationProvider _notificationProvider;

    public BackupWorker(
        ILogger logger,
        IStorageProvider storageProvider,
        IBackupNaming naming,
        IArchiveHandler archiveHandler,
        IBackupHandler backupHandler,
        IConfiguration configuration,
        INotificationProvider notificationProvider)
    {
        _logger = logger;
        _storageProvider = storageProvider;
        _naming = naming;
        _archiveHandler = archiveHandler;
        _backupHandler = backupHandler;
        _configuration = configuration;
        _notificationProvider = notificationProvider;
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


        // Start watching the time to report on any inconclusive backup
        CancellationTokenSource inconclusiveCancellation = new();
        StartInconclusiveBackupWatcher(inconclusiveCancellation.Token);

        // Do the backup
        string? archivePath = null;
        BackupReport report = new();
        try
        {
            // Generate archive
            _logger.Verbose("Starting backup archive generation...");
            archivePath = await _archiveHandler.GenerateBackupAsync("/backup/", report);
            _logger.Verbose("Backup archive generated at {path}", archivePath);

            // Get the file size
            report.ArchiveSize = _backupHandler.GetHumanBackupSize(archivePath);

            // Upload archive
            string desiredFileName = _naming.GenerateBackupFileName();
            await _storageProvider.UploadBackupAsync(archivePath, desiredFileName);
            _logger.Information("Backup complete");
            report.Status = BackupStatus.Success;
        }
        catch (Exception ex)
        {
            report.Status = BackupStatus.Failed;
            report.Errors.Add(ex.Message);
            _logger.Error("Backup failed: {exMsg}", ex.Message);
        }
        finally // Cleanup
        {
            inconclusiveCancellation.Cancel();
            if (archivePath is not null && File.Exists(archivePath))
            {
                _logger.Verbose("Local archive deleted: {path}", archivePath);
                File.Delete(archivePath);
            }
            TryNotify(report);
            _logger.Verbose("Backup iteration complete!");
        }
    }

    private void StartInconclusiveBackupWatcher(CancellationToken cancelToken)
    {
        DateTime startWatching = DateTime.UtcNow;
        int? inconclusiveAfterMinutes = _configuration.GetSection("Notify:InconclusiveAfterMinutes").Get<int?>();
        if (inconclusiveAfterMinutes is null || inconclusiveAfterMinutes < 1)
            return; // Not configured, go home
        DateTime inconclusiveAt = startWatching.AddMinutes(inconclusiveAfterMinutes.Value);
        _ = Task.Run(async () =>
        {
            while (!cancelToken.IsCancellationRequested && DateTime.UtcNow < inconclusiveAt)
                await Task.Delay(1000);
            if (cancelToken.IsCancellationRequested)
                return;
            
            // Still here, is inconclusive
            _logger.Warning("Reporting backup as inconclusive... Will continue to run until complete or manual interrupt.");
            await _notificationProvider.NotifyAsync(new BackupReport() { Status = BackupStatus.Inconclusive });
        });
    }

    
    private void TryNotify(BackupReport report)
    {
        string? minNotifyLevelString = _configuration.GetSection("Notify:MinimumNotifyLevel").Get<string?>();
        BackupStatus minNotifyLevel = string.IsNullOrEmpty(minNotifyLevelString) ? 
            BackupStatus.Success :
            (BackupStatus)Enum.Parse(typeof(BackupStatus), minNotifyLevelString);
        if (report.Status >= minNotifyLevel)
        {
            _logger.Information("Sending backup result notification...");
            _ = _notificationProvider.NotifyAsync(report);
        }
        else _logger.Information("Backup status is below minimum notify threshold. Not sending notification.");

    }
}