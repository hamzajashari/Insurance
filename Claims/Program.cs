using Azure.Messaging.ServiceBus;
using Claims.Application.Events;
using Claims.Application.Interfaces;
using Claims.Application.Repository;
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

// Create MongoClient once — reusing it avoids EF Core building a new
// internal service provider on every request (ManyServiceProvidersCreatedWarning).
var mongoClient = new MongoClient(mongoContainer.GetConnectionString());
var mongoDbName = builder.Configuration["MongoDb:DatabaseName"]!;

builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddDbContext<ClaimsContext>(options =>
    options.UseMongoDB(mongoClient, mongoDbName));

builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<ICoverRepository, CoverRepository>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<ICoverService, CoverService>();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["ServiceBus:CONNECTIONSTRING"];
    //var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING");

    return new ServiceBusClient(connectionString);
});
builder.Services.AddScoped<IAuditProducerService, AuditProducer>();
builder.Services.AddHostedService<AuditConsumer>();
//<summary>
//If I wanted to use in-memory queue instead of Azure ServiceBus,
//I could uncomment the two lines below 
//</summary>
//builder.Services.AddScoped<IAuditService, AuditService>();
//builder.Services.AddSingleton<ConcurrentQueue<AuditEvent>>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod()));

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
app.UseCors();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
    context.Database.Migrate();
}

app.Run();

public partial class Program { }
