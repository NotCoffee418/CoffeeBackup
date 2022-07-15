namespace CoffeeBackup.Common.Abstract;

public interface IStorageProvider
{
    /// <summary>
    /// Get a list of backup existing file names or paths on the storage provider.
    /// Used to determine the data of the last backup and any backup that should be removed.
    /// Files should conform to this structure: '(optionalprefix-)backup-20220101-1600.tar.gz'.
    /// </summary>
    /// <returns></returns>
    public Task<string[]> ListFilesAsync();
    
    /// <summary>
    /// Upload a file to the storage provider.
    /// </summary>
    /// <param name="localFilePath"></param>
    public Task UploadBackupAsync(string localFilePath, string? desiredFileName);

    /// <summary>
    /// Remove a file. Used to purge old backups.
    /// </summary>
    /// <param name="remoteFileName"></param>
    /// <returns></returns>
    public Task RemoveFileAsync(string remoteFileName);
}
