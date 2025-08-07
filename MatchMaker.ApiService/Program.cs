using Confluent.Kafka;
using Infrastructure.Algorithms;
using Infrastructure.Kafka;
using Infrastructure.Redis;
using MatchMaker.ApiService.DTOs;
using MatchMaker.Core.Application;
using MatchMaker.Core.Domain.Entities;
using MatchMaker.Core.Domain.Interfaces;
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

//var redisConn = builder.Configuration.GetConnectionString("redis");
//builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//    ConnectionMultiplexer.Connect(redisConn!));

builder.AddRedisClient("redis");

// Infrastructure: Redis repository
builder.Services.AddSingleton<IMatchRepository, RedisMatchRepository>();

// Kafka producer
builder.Services.AddSingleton<ProducerConfig>(sp => new ProducerConfig
{
    BootstrapServers = sp.GetRequiredService<IConfiguration>()
                             .GetConnectionString("kafka")
});
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/matchmaking/search", async (SearchRequestDto dto, IKafkaProducer producer) =>
{
    // Build Kafka message
    Console.WriteLine("POST /matchmaking/search");
    
    var request = new MatchMaker.Contracts.MatchRequestDto(dto.UserId);
    var payload = JsonSerializer.Serialize(request);

    Console.WriteLine($"{dto.UserId} {payload}");

    await producer.ProduceAsync(
        topic: KafkaEndpoints.SearchTopic,
        key: dto.UserId,
        value: payload);

    Console.WriteLine("Message was produced to SearchTopic");

    return Results.NoContent();
})
            .WithName("MatchSearch");

app.MapGet("/matchmaking/match", async (string userId, IMatchRepository matchRepository) =>
{
    var domainUser = new UserId(userId);

    Console.WriteLine($"GET /matchmaking/match?userId={userId}");
    var match = await matchRepository.GetForUserAsync(domainUser);

    if (match is null)
    {
        Console.WriteLine("Match is null");
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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
