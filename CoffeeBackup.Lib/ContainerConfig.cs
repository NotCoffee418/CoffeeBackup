namespace CoffeeBackup.Lib;

public static class ContainerConfig
{
    public static void ConfigureLib(this ContainerBuilder builder)
    {
        // Register interfaces with matching classes
        builder.BulkRegister("CoffeeBackup.Lib");
    }
}
