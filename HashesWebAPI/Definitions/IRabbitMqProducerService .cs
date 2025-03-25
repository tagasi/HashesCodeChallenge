public interface IRabbitMqProducerService {
    Task PublishMessageAsync<T>(T message);

    
}