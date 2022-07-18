namespace CoffeeBackup.Common.Abstract;

public interface INotificationProviderRegistration
{
    public bool IsProviderConfigured(IConfiguration config);
    public void Register(ContainerBuilder builder);
}
