namespace CoffeeBackup.StorageProviders.Storj;

public static class ContainerConfig
{
    public static void ConfigureStorj(this ContainerBuilder builder)
    {
        // Register interfaces with matching classes
        builder.BulkRegister("CoffeeBackup.StorageProviders.Storj");

        // Register storage provider
        builder.RegisterType<StorjProvider>().As<IStorageProvider>().SingleInstance();
    }
}
