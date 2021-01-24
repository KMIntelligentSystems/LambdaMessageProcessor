using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GraphQLWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private int executionCount = 0;

        public Worker(IServiceProvider services, ILogger<Worker> logger)
        {
            Services = services;
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Logger.Information("HEEEEEEEEEEEEEEEEEER");
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await DoWork(stoppingToken); 
            }
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
           // var service = Services.GetRequiredService<IGraphQLServiceWorker>();
            /* using (var scope = Services.CreateScope())
             {
                 var scopedProcessingService =
                     scope.ServiceProvider
                         .GetRequiredService<IGraphQLServiceWorker>();

                 await scopedProcessingService.DoWork(stoppingToken);
             }
          //  await service.DoWork(stoppingToken);*/
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
          

            return Task.CompletedTask;
        }

        public void Dispose()
        {
          //  _timer?.Dispose();
        }
    }
}
