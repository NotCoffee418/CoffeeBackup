namespace CoffeeBackup.StorageProviders.AmazonS3;

public class RegisterAmazonS3 : IStorageProviderRegistration
{
    public bool IsProviderConfigured(IConfiguration configuration)
        => !string.IsNullOrEmpty(configuration.GetSection("StorageProvider:S3:AccessKeyId").Get<string>()) &&
        !string.IsNullOrEmpty(configuration.GetSection("StorageProvider:S3:AccessKeySecret").Get<string>()) &&
        !string.IsNullOrEmpty(configuration.GetSection("StorageProvider:S3:BackupBucketName").Get<string>()) &&
        StorageClassInterpreter.IsValidStorageClassOrDefault(configuration.GetSection("StorageProvider:S3:StorageClass").Get<string>());

    public void Register(ContainerBuilder builder)
    {
        builder.BulkRegister("CoffeeBackup.StorageProviders.AmazonS3");
        builder.RegisterType<AmazonS3Provider>().As<IStorageProvider>();
    }
}
