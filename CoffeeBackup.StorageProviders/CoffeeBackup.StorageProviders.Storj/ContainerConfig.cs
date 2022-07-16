namespace CoffeeBackup.StorageProviders.Storj;

public static class ContainerConfig
{
    public static void TryConfigureStorj(this ContainerBuilder builder, IConfiguration config)
    {
        // Register interfaces with matching classes
        builder.BulkRegister("CoffeeBackup.StorageProviders.Storj");

        // Register storage provider
        builder.RegisterType<StorjProvider>().As<IStorageProvider>().SingleInstance();
    }
}
