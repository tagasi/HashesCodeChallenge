using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

public class RabbitMqProducerService : IRabbitMqProducerService, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private string QueueName;

    private readonly ILogger<RabbitMqProducerService> _logger;

    public RabbitMqProducerService(ILogger<RabbitMqProducerService> logger,IOptions<RabbitMQSettings> options)
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
    }

    public async Task PublishMessageAsync<T>(T message)
    {
        // byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        byte[] body = Encoding.UTF8.GetBytes(message.ToString());
        
        string temp = Encoding.UTF8.GetString(body);
        _logger.LogInformation($"[Original Message: {message}] [Sent message: {temp}]");

        // ✅ Instead of `CreateBasicProperties()`, use `BasicProperties()`
        //var properties = new BasicProperties { Persistent = true };
        var properties = new BasicProperties();
        properties.ContentType = "text/plain";
        properties.DeliveryMode = DeliveryModes.Persistent;
        await _channel.BasicPublishAsync(exchange: "",
                                         routingKey: QueueName,
                                         mandatory: false,
                                         basicProperties: properties,
                                         body: body);
    }

    public void Dispose()
    {
        _channel?.DisposeAsync();
        _connection?.DisposeAsync();
    }
}
