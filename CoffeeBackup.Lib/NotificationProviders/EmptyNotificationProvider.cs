namespace CoffeeBackup.Lib.NotificationProviders;

public class EmptyNotificationProvider : INotificationProvider
{
    private ILogger _logger;

    public EmptyNotificationProvider(ILogger logger)
    {
        _logger = logger;
    }
    public Task NotifyAsync(BackupReport report)
    {
        _logger.Information("Notifications are not configured.");
        return Task.CompletedTask;
    }
}
