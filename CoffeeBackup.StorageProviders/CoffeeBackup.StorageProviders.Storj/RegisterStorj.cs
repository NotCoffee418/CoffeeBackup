namespace CoffeeBackup.StorageProviders.Storj;

public class RegisterStorj : IStorageProviderRegistration
{
    public bool IsProviderConfigured(IConfiguration configuration)
        => !string.IsNullOrEmpty(configuration.GetSection("StorageProvider:Storj:StorjAccessGrant").Get<string>()) &&
        !string.IsNullOrEmpty(configuration.GetSection("StorageProvider:Storj:StorjBackupBucket").Get<string>());

    public void Register(ContainerBuilder builder)
    {
        builder.BulkRegister("CoffeeBackup.Common");
        builder.RegisterType<StorjProvider>().As<IStorageProvider>();
    }
}
