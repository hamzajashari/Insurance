using Claims.Application.Events;
using System.Collections.Concurrent;

namespace Claims.Application.Interfaces
{
    public interface IAuditService
    {
        void Enqueue(AuditEvent auditEvent);
    }
}