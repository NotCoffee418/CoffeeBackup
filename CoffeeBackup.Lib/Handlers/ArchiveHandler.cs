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
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task<string> GenerateBackupAsync(string backupSourceDir, string? specificArchivePath = null)
    {
        // Ensure trailing lash
        if (!backupSourceDir.EndsWith("/")) backupSourceDir += "/";

        // Get tmp path for our archive if not defined
        string archivePath = specificArchivePath ?? Path.GetTempFileName();

        // List all files with it's desired path in the archive
        (string SourcePath, string DestPath)[] files = Directory.GetFiles(backupSourceDir, "*", SearchOption.AllDirectories)
            .Select(x => (x, x.Substring(backupSourceDir.Length))) // Remove /backup/ in destpath
            .ToArray();

        try
        {
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
                    using (FileStream fs = File.OpenRead(sourcePath))
                        writer.Write(destPath, fs, modificationTime);
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to include file: '{sourcePath}' because of: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            if (File.Exists(archivePath))
                try
                {
                    File.Delete(archivePath);
                }
                catch (Exception delEx)
                {
                    throw new Exception("An error occurred while creating an archive and archive cleanup failed.", delEx);
                }
            throw;
        }

        // Return the path to the archive
        return Task.FromResult(archivePath);
    }
}
