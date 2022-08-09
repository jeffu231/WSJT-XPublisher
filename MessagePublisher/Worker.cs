using MessagePublisher.Mqtt;
using MessagePublisher.Service;

namespace MessagePublisher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMessagePubService _pubService;

    public Worker(ILogger<Worker> logger, IMessagePubService pubService)
    {
        _logger = logger;
        _pubService = pubService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _pubService.Start();
        while (!stoppingToken.IsCancellationRequested)
        {
            //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
        
        _pubService.Stop();
    }
}