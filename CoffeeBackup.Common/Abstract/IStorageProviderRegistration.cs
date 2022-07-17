namespace CoffeeBackup.Common.Abstract;

public interface IStorageProviderRegistration
{
    /// <summary>
    /// Check if all required configuration options are defined for this provider.
    /// If not, it will move on to the next provider
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public bool IsProviderConfigured(IConfiguration configuration);

    /// <summary>
    /// Register all dependencies and the IStorageProvider implementation as IStorageProvider
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public void Register(ContainerBuilder builder);
}
