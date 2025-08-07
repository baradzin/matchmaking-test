using Confluent.Kafka;
using Infrastructure.Algorithms;
using Infrastructure.Kafka;
using Infrastructure.Redis;
using MatchMaker.ApiService;
using MatchMaker.ApiService.DTOs;
using MatchMaker.Core.Application;
using MatchMaker.Core.Domain.Entities;
using MatchMaker.Core.Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using StackExchange.Redis;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Logging.AddConsole();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddRedisClient("redis");

// Infrastructure: Redis repository
builder.Services.AddSingleton<IMatchRepository, RedisMatchRepository>();

// Kafka producer
builder.AddKafkaConsumer<string, string>("kafka", opt =>
{
    opt.Config.GroupId = "matchmaking-service";
    opt.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
    opt.Config.AllowAutoCreateTopics = true;
});
builder.AddKafkaProducer<string, string>("kafka");

builder.Services.AddHostedService<MatchCompletionListener>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/matchmaking/search", async (SearchRequestDto dto, IProducer<string, string> producer) =>
{
    // Build Kafka message
    Console.WriteLine("POST /matchmaking/search");
    
    var request = new MatchMaker.Contracts.MatchRequestDto(dto.UserId);
    var payload = JsonSerializer.Serialize(request);

    Console.WriteLine($"{dto.UserId} {payload}");
    await producer.ProduceAsync(
        topic: KafkaEndpoints.SearchTopic, new Message<string, string> { Key = dto.UserId, Value = payload });

    Console.WriteLine("Message was produced to SearchTopic");

    return Results.NoContent();
})
            .WithName("MatchSearch");

app.MapGet("/matchmaking/match", async (string userId, IMatchRepository matchRepository) =>
{
    Console.WriteLine($"GET /matchmaking/match?userId={userId}");

    var domainUser = new UserId(userId);
    var match = await matchRepository.GetForUserAsync(domainUser);

    if (match is null)
    {
        return Results.NotFound();
    }
        
    Console.WriteLine($"Match: {match.Id}, {match.CreatedAt} for user {userId}");
    var response = new MatchResponseDto(
        match.Id.Value,
        match.Users.Select(u => u.Value).ToArray());

    return Results.Ok(response);
})
.WithName("MatchRetrieve");

app.MapDefaultEndpoints();

app.Run();
