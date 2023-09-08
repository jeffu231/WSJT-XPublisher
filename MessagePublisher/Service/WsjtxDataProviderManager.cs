using WsjtxClient.Provider;

namespace MessagePublisher.Service;

public interface IWsjtxDataProviderManager
{
    IEnumerable<IWsjtxDataProvider> WsjtxDataProviders { get; }
}

public class WsjtxDataProviderManager : IWsjtxDataProviderManager
{
    private readonly ILogger<WsjtxDataProviderManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public WsjtxDataProviderManager(IServiceProvider provider, ILogger<WsjtxDataProviderManager> logger)
    {
        _logger = logger;
        _serviceProvider = provider;
        _logger.LogDebug("Ctr {Provider}", Guid.NewGuid());
    }

    public IEnumerable<IWsjtxDataProvider> WsjtxDataProviders {
        get
        {
            var wsjtxDataProviders = _serviceProvider.GetServices<IHostedService>()
                .Where(x => x is IWsjtxDataProvider).Cast<IWsjtxDataProvider>();
            _logger.LogDebug("WsjtxDataProviders count: {Count}", wsjtxDataProviders.Count());
            return wsjtxDataProviders;
        }
    }
}