namespace CoffeeBackup.Common;

public static class ContainerConfig
{
    public static void ConfigureCommon(this ContainerBuilder builder, IConfiguration configuration)
    {
        // Register interfaces with matching classes
        builder.BulkRegister("CoffeeBackup.Common");

        // Register logger
        var loggerConfig = new LoggerConfiguration()
            .WriteTo.Console()
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Verbose();
        Log.Logger = loggerConfig.CreateLogger();
        builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();

    }
}
