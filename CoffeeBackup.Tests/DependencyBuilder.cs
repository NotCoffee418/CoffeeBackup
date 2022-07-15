namespace CoffeeBackup.Tests;

public static class DependencyBuilder
{
    public static ContainerBuilder AddMocked(
        this ContainerBuilder builder,
        Dictionary<string, string>? configOverrides = null)
    {
        // Mock IConfiguration
        var mockedConf = GetConfigurationUnderTest(configOverrides);
        builder.RegisterInstance(mockedConf).As<IConfiguration>().SingleInstance();

        // Register libraries
        builder.ConfigureCommon(mockedConf);
        builder.ConfigureLib();

        return builder;
    }

    private static IConfiguration GetConfigurationUnderTest(Dictionary<string, string>? overrides = null)
    {
        // "nested:values:can:be:added:like:this"
        var unitTestConfigurationValues = new Dictionary<string, string>
        {
            {"key", "value"},
        };

        // Set overrides
        if (overrides is not null)
            foreach ((string key, string value) in overrides)
                unitTestConfigurationValues[key] = value;

        // Build and return
        return new ConfigurationBuilder()
            .AddInMemoryCollection(unitTestConfigurationValues)
            .Build();
    }
}
