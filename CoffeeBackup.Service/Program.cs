/// ---- CONFIGURATION
IConfigurationBuilder confBuilder = new ConfigurationBuilder();
string appsettingsPath = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json");
if (File.Exists(appsettingsPath))
    confBuilder.AddJsonFile(appsettingsPath, optional: true);
confBuilder.AddEnvironmentVariables();
IConfiguration configuration = confBuilder.Build();


/// ---- SERVICE HOST
IHost host = Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        // Register config
        builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();

        // Register workers
        builder.RegisterType<BackupWorker>().As<IHostedService>().SingleInstance();

        // Register libraries
        builder.ConfigureCommon(configuration);
        builder.ConfigureLogger(configuration);
        builder.ConfigureLib();

        // Register a storage providers
        // Note for future: When trying to implement loading from assemblies that have not been accessed yet, you wont find the types
        List<IStorageProviderRegistration> storageOptions = new()
        {
            new RegisterAmazonS3(),
            new RegisterStorj(),
        };
        bool foundStorageProvider = false;
        foreach (var storageProviderReg in storageOptions)
            if (storageProviderReg.IsProviderConfigured(configuration))
            {
                foundStorageProvider = true;
                storageProviderReg.Register(builder);
                break;
            }

        // Ensure a provider is registered
        if (!foundStorageProvider)
        {
            string exMsg = "No storage provider was configured, crashing. Check the README for instructions at https://github.com/NotCoffee418/CoffeeBackup#readme";
            Log.Logger.Fatal(exMsg);
            Log.CloseAndFlush();
            throw new Exception(exMsg);
        }

        // Register a notification provider
        List<INotificationProviderRegistration> notifyOptions = new()
        {
            new SendGridProviderRegistration(),
            new EmptyNotificationProviderRegistration(), // Must exist
        };
        foreach (var notifyProviderReg in notifyOptions)
            if (notifyProviderReg.IsProviderConfigured(configuration))
            {
                foundStorageProvider = true;
                notifyProviderReg.Register(builder);
                break;
            }
    })
    .Build();


try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Logger.Fatal("CoffeeBackup.Service crashed with error {exMsg}", ex.Message);
    Log.CloseAndFlush();
    throw;
}
