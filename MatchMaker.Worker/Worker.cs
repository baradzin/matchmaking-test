using Confluent.Kafka;
using Infrastructure.Kafka;
using MatchMaker.Core.Application;
using MatchMaker.Core.Domain.Entities;
using System.Text.Json;

namespace MatchMaker.Worker
{
    public sealed class MatchmakingEngine : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;
        private readonly IMatchService _matchService;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
        private readonly ILogger<MatchmakingEngine> _logger;

        public MatchmakingEngine( IConsumer<string, string> consumer, IProducer<string, string> producer,
            IMatchService matchService, ILogger<MatchmakingEngine> logger)
        {
            _consumer = consumer;
            _producer = producer;
            _matchService = matchService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(KafkaEndpoints.SearchTopic);
            _logger.LogInformation("Subscribed to topic {Topic}", KafkaEndpoints.SearchTopic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _consumer.Consume(stoppingToken);
                    _logger.LogInformation( "Consumed message at {PartitionOffset}: {Key} → {Value}",
                        cr.TopicPartitionOffset,
                        cr.Message.Key,
                        cr.Message.Value);

                    var req = JsonSerializer.Deserialize<Contracts.MatchRequestDto>(cr.Message.Value, _jsonOptions)!;
                    var userId = new UserId(req.UserId);
                    var match = await _matchService.HandleSearchAsync(userId, stoppingToken);

                    if (match is not null)
                    {
                        var complete = new Contracts.MatchCompleteDto(
                            match.Id.Value,
                            match.Users.Select(u => u.Value).ToArray());

                        var payload = JsonSerializer.Serialize(complete, _jsonOptions);

                        await _producer.ProduceAsync(
                            KafkaEndpoints.CompleteTopic,
                            new Message<string, string>
                            {
                                Key = match.Id.Value.ToString(),
                                Value = payload
                            },
                            stoppingToken);

                        _logger.LogInformation(
                             "Produced match-complete for match {MatchId} to topic {Topic}",
                             match.Id, KafkaEndpoints.CompleteTopic);
                    }
                }
                catch (ConsumeException cex)
                {
                    _logger.LogError($"Kafka consume error: {cex.Error.Reason}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
            base.Dispose();
        }
    }
}
