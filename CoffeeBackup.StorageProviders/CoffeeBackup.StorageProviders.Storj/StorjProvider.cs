using Serilog;
using uplink.NET.Models;
using uplink.NET.Services;
using static CoffeeBackup.Common.Data.Enums;

namespace CoffeeBackup.StorageProviders.Storj;

public class StorjProvider : IStorageProvider
{
    private IConfiguration _configuration;
    private ILogger _logger;

    public StorjProvider(
        IConfiguration configuration,
        ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    

    public async Task<string[]> ListFilesAsync()
    {
        _logger.Verbose("Storj: Listing remote files");
        (Bucket bucket, ObjectService objectService) = await GetBucketAndObjectServiceAsync();
        
        // Get objects in bucket
        ObjectList objects = await objectService.ListObjectsAsync(bucket, new());
        return objects.Items.Select(x => x.Key).ToArray();
    }

    public async Task RemoveFileAsync(string remoteFileName)
    {
        _logger.Verbose("Storj: Removing remote backup archive at {path}", remoteFileName);
        
        // Verify file's existance
        string[] existingFiles = await ListFilesAsync();
        if (!existingFiles.ToList().Contains(remoteFileName))
        {
            _logger.Warning("Attempted to clean backup {file} but it did not exist", remoteFileName);
            return;
        }

        // Remove the file
        (Bucket bucket, ObjectService objectService) = await GetBucketAndObjectServiceAsync();
        await objectService.DeleteObjectAsync(bucket, remoteFileName);
    }

    public async Task UploadBackupAsync(string localFilePath, string? desiredFileName)
    {
        _logger.Verbose("Storj: Uploading backup from {file}", localFilePath);
        (Bucket bucket, ObjectService objectService) = await GetBucketAndObjectServiceAsync();
        string fileName = desiredFileName ?? Path.GetFileName(localFilePath);
        using (FileStream stream = File.OpenRead(localFilePath))
        {
            var uploadOperation = await objectService.UploadObjectAsync(bucket, fileName, new UploadOptions(), stream, false);
            await uploadOperation.StartUploadAsync();
        }        
    }

    private async Task<(Bucket, ObjectService)> GetBucketAndObjectServiceAsync()
    {
        Access access = new Access(
            _configuration.GetRequiredSection("StorageProvider:Storj:StorjAccessGrant").Get<string>());
        var bucketService = new BucketService(access);
        Task<Bucket> bucketTask = bucketService.GetBucketAsync(
            _configuration.GetRequiredSection("StorageProvider:Storj:StorjBackupBucket").Get<string>());
        return (await bucketTask, new ObjectService(access));
    }

}