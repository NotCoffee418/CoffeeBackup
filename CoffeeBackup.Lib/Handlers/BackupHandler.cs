using ByteSizeLib;

namespace CoffeeBackup.Lib.Handlers;

public class BackupHandler : IBackupHandler
{
    private IConfiguration _configuration;
    private IStorageProvider _storageProvider;
    private ILogger _logger;
    private IBackupNaming _naming;

    public BackupHandler(
        IConfiguration configuration,
        IStorageProvider storageProvider,
        ILogger logger,
        IBackupNaming naming)
    {
        _configuration = configuration;
        _storageProvider = storageProvider;
        _logger = logger;
        _naming = naming;
    }

    public string GetHumanBackupSize(string archivePath)
    {
        if (!File.Exists(archivePath))
            return "Unknown";
        
        // Get size in different display formats
        List<(string Identifier, double Size)> roundedSizes = new();
        ByteSize bytesize = ByteSize.FromBytes(new FileInfo(archivePath).Length);
        roundedSizes.Add(("TB", Math.Round(bytesize.TeraBytes, 2)));
        roundedSizes.Add(("GB", Math.Round(bytesize.GigaBytes, 2)));
        roundedSizes.Add(("MB", Math.Round(bytesize.MegaBytes, 2)));
        roundedSizes.Add(("KB", Math.Round(bytesize.KiloBytes, 2)));

        // Choose the ideal one and return it
        foreach ((string Identifier, double Size) roundedSize in roundedSizes)
            if (roundedSize.Size > 1)
                return $"{roundedSize.Size} {roundedSize.Identifier}";

        // Still here, return bytes
        return $"{bytesize.Bytes} bytes";
    }

    public async Task TryCleanOldBackups(string[]? allObjects)
    {
        if (allObjects is null)
        {
            allObjects = await _storageProvider.ListFilesAsync();
        }

        // Try cleanup old backups if defined in Service
        int? cleanupAfterDays = _configuration.GetSection("BackupSettings:CleanupAfterDays").Get<int?>();
        if (cleanupAfterDays is not null && cleanupAfterDays > 0)
        {
            _logger.Verbose("Cleaning old backups...");
            DateTime oldBackupTimeBefore = DateTime.UtcNow.AddDays(cleanupAfterDays.Value * -1);
            string[] filesToClean = allObjects
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
