public interface IMessageProcessor
{
    Task ProcessAsync(MessageObj message, CancellationToken stoppingToken);
}