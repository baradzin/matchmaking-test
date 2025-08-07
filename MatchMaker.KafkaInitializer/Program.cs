using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
var config = builder.Configuration;
var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("KafkaInit");

var bootstrapServers = config.GetConnectionString("kafka");

logger.LogInformation("Connecting to Kafka at: {BootstrapServers}", bootstrapServers);

var adminConfig = new AdminClientConfig
{
    BootstrapServers = bootstrapServers
};

using var adminClient = new AdminClientBuilder(adminConfig).Build();

try
{
    adminClient.CreateTopicsAsync(
    [
        new TopicSpecification { Name = "matchmaking.request", NumPartitions = 1, ReplicationFactor = 1 },
        new TopicSpecification { Name = "matchmaking.complete", NumPartitions = 1, ReplicationFactor = 1 },
    ]).Wait();

    logger.LogInformation("Kafka topics created successfully.");
}
catch (CreateTopicsException ex)
{
    foreach (var result in ex.Results)
    {
        if (result.Error.Code != ErrorCode.TopicAlreadyExists)
        {
            logger.LogError("Failed to create topic {Topic}: {Reason}", result.Topic, result.Error.Reason);
            throw;
        }

        logger.LogWarning("Topic already exists: {Topic}", result.Topic);
    }
}

Environment.Exit(0);