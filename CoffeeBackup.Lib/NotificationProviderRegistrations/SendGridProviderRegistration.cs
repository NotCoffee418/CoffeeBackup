namespace CoffeeBackup.Lib.NotificationProviderRegistrations;

/// <summary>
/// This is the default notification provider if no other is registered, as it is optional.
/// </summary>
public class SendGridProviderRegistration : INotificationProviderRegistration
{
    public bool IsProviderConfigured(IConfiguration config)
        => !string.IsNullOrEmpty(config.GetSection("Notify:SendGrid:ApiKey").Get<string>()) &&
        !string.IsNullOrEmpty(config.GetSection("Notify:SendGrid:FromEmail").Get<string>()) &&
        !string.IsNullOrEmpty(config.GetSection("Notify:SendGrid:FromName").Get<string>()) &&
        !string.IsNullOrEmpty(config.GetSection("Notify:SendGrid:ToEmail").Get<string>());

    public void Register(ContainerBuilder builder) 
    {
        builder.RegisterType<SendGridProvider>().As<INotificationProvider>();
    }
}
