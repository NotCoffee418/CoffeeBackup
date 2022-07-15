namespace CoffeeBackup.Common;

public static class ContainerConfig
{
    public static void ConfigureCommon(this ContainerBuilder builder, IConfiguration configuration)
    {
        // Register interfaces with matching classes
        builder.BulkRegister("CoffeeBackup.Common");
    }

    // Cannot be run by azure functions
    public static void ConfigureLogger(this ContainerBuilder builder, IConfiguration configuration)
    {
        // Ensure log directory exists
        // Docker should mount this like so:
        // - /var/log/coffeebackup/:/app/logs/
        string logsDir = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs");
        if (!Directory.Exists(logsDir))
            Directory.CreateDirectory(logsDir);

        // Register logger
        var loggerConfig = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Async(x => x.File(Path.Join(logsDir, "log-.txt"), rollingInterval: RollingInterval.Month))
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Verbose();
        Log.Logger = loggerConfig.CreateLogger();
        builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
    }
}
