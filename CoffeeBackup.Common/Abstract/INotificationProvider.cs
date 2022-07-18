namespace CoffeeBackup.Common.Abstract;

public interface INotificationProvider
{
    public Task NotifyAsync(BackupReport report);
}
