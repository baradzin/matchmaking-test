using Confluent.Kafka;
using Infrastructure.Algorithms;
using Infrastructure.Kafka;
using Infrastructure.Redis;
using MatchMaker.Core.Application;
using MatchMaker.Core.Domain.Interfaces;
using MatchMaker.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

// (опционально) подтянуть все стандартные логи / конфиг / health defaults
builder.AddServiceDefaults();

// Kafka Producer / Consumer
builder.AddKafkaProducer<string, string>("kafka");
builder.AddKafkaConsumer<string, string>("kafka", opt =>
{
    opt.Config.GroupId = "matchmaking-worker";
    opt.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
    opt.Config.AllowAutoCreateTopics = true;
});

// Redis
//var redisConn = builder.Configuration.GetConnectionString("redis");
//builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//    ConnectionMultiplexer.Connect(redisConn!));

builder.AddRedisClient("redis");

// Доменные сервисы
builder.Services.AddSingleton<IMatchService, MatchService>();

builder.Services.AddSingleton<IMatchAlgorithm>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RoundRobinAlgorithm>>();
    return new RoundRobinAlgorithm(logger, usersPerMatch: 3);
});

builder.Services.AddSingleton<IMatchRepository, RedisMatchRepository>();

// Ваш фоновой engine
builder.Services.AddHostedService<MatchmakingEngine>();

builder.Build().Run();