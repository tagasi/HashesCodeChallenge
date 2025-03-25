public class MessageProcessor : IMessageProcessor
{
    private readonly IDBHelper _dbService;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly ILogger<MessageProcessor> _logger;

    public MessageProcessor(IDBHelper dbService, IRabbitMQService rabbitMQService, ILogger<MessageProcessor> logger)
    {
        _dbService = dbService;
        _rabbitMQService = rabbitMQService;
        _logger = logger;
    }

    public async Task ProcessAsync(MessageObj message, CancellationToken stoppingToken)
    {
        try
        {
            await _dbService.InsertHashAsync(message.message);
            _logger.LogInformation($"Processed message: {message.message}");
            await _rabbitMQService.AcknowledgeMessageAsync(message.deliveryTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message.");
            await _rabbitMQService.RejectMessageAsync(message.deliveryTag);
        }
    }
}