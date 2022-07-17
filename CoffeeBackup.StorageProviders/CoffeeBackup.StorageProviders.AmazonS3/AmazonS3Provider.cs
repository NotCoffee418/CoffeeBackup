namespace CoffeeBackup.StorageProviders.AmazonS3;

public class AmazonS3Provider : IStorageProvider
{
    public AmazonS3Provider(
        IConfiguration configuration,
        ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
        s3Client = new AmazonS3Client(
            configuration.GetRequiredSection("StorageProvider:S3:AccessKeyId").Get<string>(),
            configuration.GetRequiredSection("StorageProvider:S3:AccessKeySecret").Get<string>());
        BucketName = _configuration.GetRequiredSection("StorageProvider:S3:BackupBucketName").Get<string>();
    }

    private IConfiguration _configuration;
    private ILogger _logger;
    static IAmazonS3 s3Client;
    
    public string BucketName { get; }

    public async Task<string[]> ListFilesAsync()
    {
        ListObjectsV2Request request = new()
        {
            BucketName = this.BucketName,
        };
        ListObjectsV2Response response = await s3Client.ListObjectsV2Async(request);
        return response.S3Objects.Select(x => x.Key).ToArray();
    }

    public Task RemoveFileAsync(string remoteFileName)
    {
        _logger.Warning("CoffeeBackup does not support removing old backups on Amazon S3. " +
            "Please set up a lifecycle policy to clean backups instead.");
        return Task.CompletedTask;
    }

    public Task UploadBackupAsync(string localFilePath, string? desiredFileName)
    {
        _logger.Verbose(messageTemplate: "S3: Uploading backup from {file}", localFilePath);
        PutObjectRequest request = new()
        {
            BucketName = this.BucketName,
            FilePath = localFilePath,
            Key = desiredFileName,
            StorageClass = StorageClassInterpreter.GetStorageClassFromName(
                _configuration.GetSection("StorageProvider:S3:StorageClass").Get<string>())
        };
        return s3Client.PutObjectAsync(request);
    }
}