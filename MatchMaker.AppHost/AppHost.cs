using Aspire.Hosting;
using Aspire.Hosting.Redis;
using Aspire.Confluent.Kafka;
using System.Collections.Immutable;
var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka("kafka");
var redis = builder.AddRedis("redis");

var kafkaInit = builder.AddProject<Projects.MatchMaker_KafkaInitializer>("kafka-init")
    .WithReference(kafka)
    .WaitFor(kafka)
    .ExcludeFromManifest()
    .WithInitialState(new CustomResourceSnapshot
    {
        ResourceType = "kafka-init",
        IsHidden = true,
        CreationTimeStamp = DateTime.UtcNow,
        State = KnownResourceStates.NotStarted,
        Properties = []
    });

var apiService = builder.AddProject<Projects.MatchMaker_ApiService>("service")
    .WithHttpHealthCheck("/health")
    .WaitForCompletion(kafkaInit)
    .WithReference(kafka)
    .WithReference(redis)
    .WaitFor(redis);

builder.AddProject<Projects.MatchMaker_Worker>("worker")
    .WithReplicas(2)
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(kafka);

builder.Build().Run();
