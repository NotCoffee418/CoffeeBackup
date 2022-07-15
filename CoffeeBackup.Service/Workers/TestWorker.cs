using CoffeeBackup.Common.Abstract;
using CoffeeBackup.Common.DataAccess;
using static CoffeeBackup.Common.Data.Enums;

namespace CoffeeBackup.Service.Workers
{
    internal class TestWorker : BackgroundService
    {
        private ILogger _logger;
        private IConfigAccess _configAccess;
        private IStorageProvider _storageProvider;

        public TestWorker(
            ILogger logger,
            IConfigAccess configAccess,
            IStorageProvider storageProvider)
        {
            _logger = logger;
            _configAccess = configAccess;
            _storageProvider = storageProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        }
    }
}
