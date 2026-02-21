using Azure.Messaging.ServiceBus;
using Claims.Application.Events;
using Claims.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Claims.Application.Services.AuditServiceBuss
{
    public class AuditProducer : IAuditProducerService
    {
        private readonly ServiceBusSender _sender;

        public AuditProducer(ServiceBusClient client, IConfiguration config)
        {
            //var queueName = config["ServiceBus:QueueName"];
            var queueName = Environment.GetEnvironmentVariable("QUEUENAME");
            _sender = client.CreateSender(queueName);
        }

        public async Task EnqueueAsync(AuditEvent evt)
        {
            var message = new ServiceBusMessage(
                JsonSerializer.Serialize(evt));

            await _sender.SendMessageAsync(message);
        }
    }
}
