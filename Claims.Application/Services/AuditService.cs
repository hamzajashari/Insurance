using Claims.Application.Events;
using Claims.Application.Interfaces;
using Claims.Domain.Audit;
using Claims.Infrastructure.DbContexts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Claims.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly ConcurrentQueue<AuditEvent> _queue;

        public AuditService(ConcurrentQueue<AuditEvent> queue)
        {
            _queue = queue;
        }

        public void Enqueue(AuditEvent auditEvent)
        {
            _queue.Enqueue(auditEvent);
        }
    }
}
