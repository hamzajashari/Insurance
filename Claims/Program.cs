using Azure.Messaging.ServiceBus;
using Claims.Application.Events;
using Claims.Application.Interfaces;
using Claims.Application.Services;
using Claims.Application.Services.AuditServiceBuss;
using Claims.Application.Services.InMemoryQueue;
using Claims.Controllers;
using Claims.Infrastructure.DbContexts;
using Claims.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Testcontainers.MongoDb;
using Testcontainers.MsSql;

var builder = WebApplication.CreateBuilder(args);

// Start Testcontainers for SQL Server and MongoDB
var sqlContainer = (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        : new()

    ).Build();

var mongoContainer = new MongoDbBuilder()
    .WithImage("mongo:latest")
    .Build();

await sqlContainer.StartAsync();
await mongoContainer.StartAsync();

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<AuditContext>(options =>
    options.UseSqlServer(sqlContainer.GetConnectionString()));

builder.Services.AddDbContext<ClaimsContext>(options =>
{
    var client = new MongoClient(mongoContainer.GetConnectionString());
    var database = client.GetDatabase(builder.Configuration["MongoDb:DatabaseName"]); // Use a default/test database name
    options.UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName);
});

builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICoverService, CoverService>();
builder.Services.AddSingleton<ConcurrentQueue<AuditEvent>>();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["ServiceBus:ConnectionString"];

    return new ServiceBusClient(connectionString);
});
builder.Services.AddScoped<IAuditProducerService, AuditProducer>();
builder.Services.AddHostedService<AuditConsumer>();
//<summary>
//If I wanted to use in-memory queue instead of Azure ServiceBus,
//I could uncomment the line below and inject ConcurrentQueue<AuditEvent> into AuditBackgroundService and AuditProducer
//</summary>


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
    context.Database.Migrate();
}

app.Run();

public partial class Program { }
