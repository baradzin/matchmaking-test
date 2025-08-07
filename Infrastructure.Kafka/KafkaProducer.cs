using Confluent.Kafka;

namespace Infrastructure.Kafka
{
    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(ProducerConfig config)
            => _producer = new ProducerBuilder<string, string>(config).Build();

        public Task ProduceAsync(string topic, string key, string value)
            => _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = value });

        public void Dispose() => _producer.Dispose();
    }

    public interface IKafkaProducer
    {
        Task ProduceAsync(string topic, string key, string value);
    }
}
