using Claims.Application.Events;
using Claims.Application.Interfaces;

namespace Claims.Application.Services.InMemoryQueue
{
    public class InMemoryAuditProducer : IAuditProducerService
    {
        private readonly AuditBackgroundService _backgroundService;

        public InMemoryAuditProducer(AuditBackgroundService backgroundService)
        {
            _backgroundService = backgroundService;
        }

        public Task EnqueueAsync(AuditEvent auditEvent)
        {
            _backgroundService.Enqueue(auditEvent);
            return Task.CompletedTask;
        }
    }
}
