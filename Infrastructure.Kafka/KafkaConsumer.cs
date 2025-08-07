using Confluent.Kafka;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Kafka
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly Func<string, Task> _handleMessage;

        public KafkaConsumer(ConsumerConfig config, Func<string, Task> handleMessage)
        {
            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _handleMessage = handleMessage;
            _consumer.Subscribe(KafkaEndpoints.SearchTopic);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);
                await _handleMessage(result.Message.Value);
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
