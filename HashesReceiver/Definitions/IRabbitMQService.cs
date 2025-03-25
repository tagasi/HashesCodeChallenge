using System.Collections.Concurrent;

public interface IRabbitMQService {
    Task StartListeningAsync(ConcurrentQueue<MessageObj> _messageQueue);
    Task AcknowledgeMessageAsync(ulong deliveryTag);
    Task RejectMessageAsync(ulong deliveryTag);
    ValueTask DisposeAsync();
}