namespace CoffeeBackup.StorageProviders.Storj;

public class StorjProvider : IStorageProvider
{
    public Task<string[]> ListBackupFilesAsync()
    {
        throw new NotImplementedException();
    }

    public Task RemoveFileAsync(string remoteFileName)
    {
        throw new NotImplementedException();
    }

    public Task UploadBackupAsync(string localFilePath)
    {
        throw new NotImplementedException();
    }
}