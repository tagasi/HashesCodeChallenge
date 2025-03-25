namespace HashesReceiver;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Worker : BackgroundService
{
    private const int ParallelThreads = 4;
    private readonly ILogger<Worker> _logger;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly IMessageProcessor _messageProcessor;
    private readonly ConcurrentQueue<MessageObj> _messageQueue = new();

    public Worker(ILogger<Worker> logger, IRabbitMQService rabbitMQService, IMessageProcessor messageProcessor)
    {
        _logger = logger;
        _rabbitMQService = rabbitMQService;
        _messageProcessor = messageProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started.");
        
        // Start RabbitMQ message listening in a background task
        _ = _rabbitMQService.StartListeningAsync(_messageQueue);
        // Process messages in 4 Parallel Threads
        await Parallel.ForEachAsync(Enumerable.Range(1, ParallelThreads), stoppingToken, async (_, _) =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_messageQueue.TryDequeue(out var message))
                {
                    try
                    {
                        await _messageProcessor.ProcessAsync(message, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Message processing failed.");
                    }
                }
                else
                {
                    await Task.Delay(100, stoppingToken); // Prevent CPU overuse
                }
            }
        });

        _logger.LogInformation("Worker stopped.");
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping Worker...");
        await base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _rabbitMQService.DisposeAsync().AsTask().Wait();
        base.Dispose();
    }
}
