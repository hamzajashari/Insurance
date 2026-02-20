using Claims.Application.Events;
using Claims.Infrastructure.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

public class AuditBackgroundService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ConcurrentQueue<AuditEvent> _queue = new();

    public AuditBackgroundService(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void Enqueue(AuditEvent evt) => _queue.Enqueue(evt);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (_queue.TryDequeue(out var evt))
            {
                using var scope = _provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AuditContext>();
                db.Add(evt.ToEntity());
                await db.SaveChangesAsync();
            }
            await Task.Delay(100); // small delay to reduce CPU
        }
    }
}