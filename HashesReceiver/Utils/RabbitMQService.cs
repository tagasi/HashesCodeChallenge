using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

public class RabbitMQService: IRabbitMQService {
        private readonly ILogger<RabbitMQService> _logger;
        private readonly RabbitMQSettings _settings;
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private string QueueName;
        private const int ParallelThreads = 4;

        public RabbitMQService(ILogger<RabbitMQService> logger, IOptions<RabbitMQSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
            QueueName = _settings.QueueName;

            var factory = new ConnectionFactory
            {
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                HostName = _settings.Host
            };

            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;

            _channel.QueueDeclareAsync(queue: QueueName,
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null).Wait();

            _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: (ushort)ParallelThreads, global: false).Wait();
        }

        public async Task StartListeningAsync(ConcurrentQueue<MessageObj> _messageQueue)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                MessageObj instMessage = new MessageObj(){message=message,deliveryTag=ea.DeliveryTag};
                _messageQueue.Enqueue(instMessage);
                await Task.CompletedTask; // Avoid compiler warnings
            };

            // this consumer tag identifies the subscription
            string consumerTag =  await _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);
        }

        public async Task AcknowledgeMessageAsync(ulong deliveryTag)
        {
            await _channel.BasicAckAsync(deliveryTag, multiple: false);
        }

    public async Task RejectMessageAsync(ulong deliveryTag)
    {
        await _channel.BasicNackAsync(deliveryTag, false, true);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}