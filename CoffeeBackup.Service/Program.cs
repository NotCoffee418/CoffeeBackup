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
    })
    .Build();

await host.RunAsync();
