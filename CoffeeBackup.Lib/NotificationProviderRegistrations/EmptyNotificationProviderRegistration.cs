namespace CoffeeBackup.Lib.NotificationProviderRegistrations;

/// <summary>
/// This is the default notification provider if no other is registered, as it is optional.
/// </summary>
public class EmptyNotificationProviderRegistration : INotificationProviderRegistration
{
    public bool IsProviderConfigured(IConfiguration config)
        => true;

    public void Register(ContainerBuilder builder) 
    {
        builder.RegisterType<EmptyNotificationProvider>().As<INotificationProvider>();
    }
}
