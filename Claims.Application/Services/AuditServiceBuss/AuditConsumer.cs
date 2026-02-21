using Azure.Messaging.ServiceBus;
using Claims.Application.Events;
using Claims.Infrastructure.DbContexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Claims.Application.Services.AuditServiceBuss
{
 
    public class AuditConsumer : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceProvider _provider;

        public AuditConsumer(
            ServiceBusClient client,
            IConfiguration config,
            IServiceProvider provider)
        {
            _provider = provider;

            //var queueName = config["ServiceBus:QueueName"];

            var queueName = Environment.GetEnvironmentVariable("QueueName");

            _processor = client.CreateProcessor(queueName);

            _processor.ProcessMessageAsync += HandleMessage;
            _processor.ProcessErrorAsync += HandleError;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _processor.StartProcessingAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessage(ProcessMessageEventArgs args)
        {
            var evt = JsonSerializer.Deserialize<AuditEvent>(
                args.Message.Body.ToString());

            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuditContext>();

            db.Add(evt!.ToEntity());
            await db.SaveChangesAsync();

            await args.CompleteMessageAsync(args.Message);
        }

        private Task HandleError(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception);
            return Task.CompletedTask;
        }
    }
}
