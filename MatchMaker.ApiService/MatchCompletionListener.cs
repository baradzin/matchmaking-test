using Confluent.Kafka;
using Infrastructure.Kafka;
using MatchMaker.Contracts;
using MatchMaker.Core.Domain.Entities;
using MatchMaker.Core.Domain.Interfaces;
using System.Text.Json;

namespace MatchMaker.ApiService
{
    public class MatchCompletionListener : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IMatchRepository _matchRepository;
        private readonly ILogger<MatchCompletionListener> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public MatchCompletionListener(
            IConsumer<string, string> consumer,
            IMatchRepository matchRepository,
            ILogger<MatchCompletionListener> logger)
        {
            _consumer = consumer;
            _matchRepository = matchRepository;
            _logger = logger;
            _consumer.Subscribe(KafkaEndpoints.CompleteTopic);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            while (!stoppingToken.IsCancellationRequested)
            {
                var cr = _consumer.Consume(stoppingToken);
                _logger.LogInformation("Consumed match-complete {TopicPartitionOffset}: {Key} → {Value}",
                    cr.TopicPartitionOffset, cr.Message.Key, cr.Message.Value);
                var dto = JsonSerializer.Deserialize<MatchCompleteDto>(cr.Message.Value, _jsonOptions)!;
                var match = new Match(
                    new MatchId(dto.MatchId),
                    dto.UserIds.Select(id => new UserId(id)).ToList().AsReadOnly(),
                    DateTimeOffset.UtcNow);

                await _matchRepository.SaveAsync(match, stoppingToken);
                _logger.LogInformation("Saved match {MatchId} for users: {Users}",
                    dto.MatchId, string.Join(", ", dto.UserIds));
            }
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
