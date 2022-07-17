namespace CoffeeBackup.StorageProviders.AmazonS3;

public class AmazonS3Provider : IStorageProvider
{
    private IConfiguration _configuration;
    private ILogger _logger;

    public AmazonS3Provider(
        IConfiguration configuration,
        ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Decide on region name
        string? regionName = configuration.GetSection("StorageProvider:S3:RegionName").Get<string>();
        if (string.IsNullOrEmpty(regionName))
            regionName = "us-east-1";

        // Set up client
        Client = new AmazonS3Client(
            configuration.GetRequiredSection("StorageProvider:S3:AccessKeyId").Get<string>(),
            configuration.GetRequiredSection("StorageProvider:S3:AccessKeySecret").Get<string>(),
            Amazon.RegionEndpoint.GetBySystemName(regionName));

        // Preemtively set bucket name
        BucketName = _configuration.GetRequiredSection("StorageProvider:S3:BackupBucketName").Get<string>();
    }
    
    private IAmazonS3 Client { get; init; }    
    private string BucketName { get; init; }

    public async Task<string[]> ListFilesAsync()
    {
        ListObjectsV2Request request = new()
        {
            BucketName = this.BucketName,
        };
        ListObjectsV2Response response = await Client.ListObjectsV2Async(request);
        return response.S3Objects.Select(x => x.Key).ToArray();
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
        return Client.PutObjectAsync(request);
    }

    public Task RemoveFileAsync(string remoteFileName)
    {
        _logger.Warning("CoffeeBackup does not support removing old backups on Amazon S3. " +
            "Please set up a lifecycle policy to clean backups instead.");
        return Task.CompletedTask;
    }

}