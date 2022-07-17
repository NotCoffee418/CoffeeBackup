namespace CoffeeBackup.StorageProviders.Storj;

public class RegisterStorj : IStorageProviderRegistration
{
    public bool IsProviderConfigured(IConfiguration configuration)
        => !string.IsNullOrEmpty(configuration.GetSection("StorageProvider:Storj:AccessGrant").Get<string>()) &&
        !string.IsNullOrEmpty(configuration.GetSection("StorageProvider:Storj:BackupBucketName").Get<string>());

    public void Register(ContainerBuilder builder)
    {
        builder.BulkRegister("CoffeeBackup.StorageProviders.Storj");
        builder.RegisterType<StorjProvider>().As<IStorageProvider>().SingleInstance();
    }
}
