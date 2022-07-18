namespace CoffeeBackup.Lib.Handlers;

public class ArchiveHandler : IArchiveHandler
{
    private ILogger _logger;

    public ArchiveHandler(ILogger logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Create a backup and return the path to the backup file.
    /// </summary>
    /// <param name="backupSourceDir">Root of the archive</param>
    /// <param name="specificArchivePath">Optionally specify an output path, otherwise a temp path is used</param>
    /// <returns>Path to the temporary archive file/returns>
    public Task<string> GenerateBackupAsync(string backupSourceDir, BackupReport workingReport, string? specificArchivePath = null)
    {
        // Ensure trailing lash
        if (!backupSourceDir.EndsWith("/")) backupSourceDir += "/";

        // Validate directory
        if (!Directory.Exists(backupSourceDir))
            throw new Exception($"No backup paths mounted to '{backupSourceDir}'. See documentation for configuring backups.");

        // List all files with it's desired path in the archive
        (string SourcePath, string DestPath)[] files = Directory.GetFiles(backupSourceDir, "*", SearchOption.AllDirectories)
            .Select(x => (x, x.Substring(backupSourceDir.Length))) // Remove /backup/ in destpath
            .ToArray();

        // Don't attempt to backup if no backup paths are bound
        if (files.Length == 0)
            throw new Exception("Found no files to back up. Please ensure that the desired backup locations " +
                "are correcly mounted to the internal /backup/ directory");
        else _logger.Verbose("Backing up {count} files.", files.Length);

        string? archivePath = null;
        try
        {
            // Get tmp path for our archive if not defined
            archivePath = specificArchivePath ?? Path.GetTempFileName();
            _logger.Verbose("Creating backup archive at {path}", archivePath);
            
            // Define writer
            WriterOptions wo = new(CompressionType.GZip)
            {
                LeaveStreamOpen = true
            };

            // Add the files to the archive
            using Stream compressionStream = File.OpenWrite(archivePath);
            using var writer = WriterFactory.Open(compressionStream, ArchiveType.Tar, wo);
            foreach (var (sourcePath, destPath) in files)
            {
                DateTime modificationTime = new FileInfo(sourcePath).LastWriteTimeUtc;
                try
                {
                    _logger.Verbose("Including file: {path}", destPath);
                    using (FileStream fs = File.OpenRead(sourcePath))
                        writer.Write(destPath, fs, modificationTime);
                    workingReport.AddedFiles.Add(destPath);
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to include file: '{sourcePath}' because of: {ex.Message}");
                    workingReport.IgnoredFiles.Add((destPath, ex.Message));
                }
            }
        }
        catch (Exception backupEx)
        {
            workingReport.Errors.Add(backupEx.Message);
            if (!string.IsNullOrEmpty(archivePath) && File.Exists(archivePath))
                try
                {
                    File.Delete(archivePath);
                }
                catch (Exception delEx)
                {
                    throw new Exception("An error occurred while creating an archive and archive cleanup failed.", delEx);
                    workingReport.Errors.Add(delEx.Message);
                }
            throw;
        }

        // Return the path to the archive
        return Task.FromResult(archivePath);
    }
}
