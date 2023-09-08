using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using MessagePublisher.Models;
using Microsoft.Extensions.Caching.Memory;
using WsjtxClient.Events;
using WsjtxClient.Models;
using WsjtxClient.Provider;

namespace MessagePublisher.Service;

public class DxMapsSpotService: BackgroundService
{
    private readonly ILogger<DxMapsSpotService> _logger;
    private readonly IConfiguration _config;
    private readonly IWsjtxDataProviderManager _dataProviderManager;
    private readonly ConcurrentDictionary<string, WsjtxStatus> _wsjtxInstance;
    private readonly Dictionary<string, MemoryCache> _callCaches;
    private readonly UdpClient _udpClient;
    private bool _isEnabled;

    public DxMapsSpotService(IWsjtxDataProviderManager wsjtxDataProviderManager, IConfiguration configuration, ILogger<DxMapsSpotService> logger)
    {
        _logger = logger;
        _config = configuration;
        _dataProviderManager = wsjtxDataProviderManager;
        _wsjtxInstance = new ConcurrentDictionary<string, WsjtxStatus>();
        _callCaches = new Dictionary<string, MemoryCache>();
        _logger.LogDebug("DxMaps service ctr");
        _udpClient = new UdpClient();
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if(_isEnabled == value) return;
            _isEnabled = value;
            if (_isEnabled)
            {
                foreach (var wsjtxDataProvider in _dataProviderManager.WsjtxDataProviders)
                {
                    _logger.LogDebug("Subscribing to provider {Id}", wsjtxDataProvider.Id);
                    wsjtxDataProvider.DecodeReceived += DataProviderManagerOnDataReceived;
                    wsjtxDataProvider.StatusReceived += DataProviderManagerOnStatusReceived;
                }
                
            }
            else
            {
                foreach (var wsjtxDataProvider in _dataProviderManager.WsjtxDataProviders)
                {
                    _logger.LogDebug("UnSubscribing to provider {Id}", wsjtxDataProvider.Id);
                    wsjtxDataProvider.DecodeReceived -= DataProviderManagerOnDataReceived;
                    wsjtxDataProvider.StatusReceived -= DataProviderManagerOnStatusReceived;
                }
            }
            
            _logger.LogInformation("DxMaps Enabled {Disabled}", _isEnabled);
            _logger.LogInformation("DxMaps Send Spot Enabled {Enabled}", 
                _config.GetValue<bool>("DxMaps:SendSpot"));
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("DxMaps service execute starting");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            IsEnabled = _config.GetValue<bool>("DxMaps:Enabled");
            await Task.Delay(1000, stoppingToken);
        }
        
        _logger.LogDebug("DxMaps service execute finishing");
    }
    
    private async void DataProviderManagerOnDataReceived(object? sender, WsjtxDecodeEventArgs e)
    {
        _logger.LogDebug("WSJT-X Data Message {data}", e.Decode);

        var decode = e.Decode;
        if (!decode.LowConfidence && decode.IsExchangeGrid && IsValidCall(decode.Callsign))
        {
            if (_wsjtxInstance.TryGetValue(decode.Id, out var instance))
            {
                if (_callCaches.TryGetValue(decode.Id, out var callCache))
                {
                    if (!callCache.TryGetValue(decode.Callsign, out _))
                    {
                        _logger.LogDebug("Call {Call} not found in cache, sending spot", decode.Callsign);
                        var spot = await CreateSpot(instance, decode);
                        callCache.Set(decode.Callsign, spot, TimeSpan.FromMinutes(15));
                    }
                    else
                    {
                        _logger.LogDebug("Call {Call} found in cache, not sending spot", decode.Callsign);
                    }
                }
            }
        }
        
    }

    private void DataProviderManagerOnStatusReceived(object? sender, WsjtxStatusEventArgs e)
    {
        _logger.LogDebug("WSJT-X Status Message {Status}", e.Status);
        
        if (_wsjtxInstance.TryGetValue(e.Status.Id, out var status))
        {
            var freqDiff = Math.Abs((double)(status.DialFrequency - e.Status.DialFrequency));
            if ( freqDiff > 20 || status.Mode != e.Status.Mode)
            {
                if (_callCaches.TryGetValue(e.Status.Id, out var cache))
                {
                    cache.Compact(1);
                }
            }
            _wsjtxInstance[e.Status.Id] = e.Status;
        }
        else
        {
            _wsjtxInstance.TryAdd(e.Status.Id, e.Status);
            _callCaches.TryAdd(e.Status.Id, new MemoryCache(new MemoryCacheOptions()));
        }
    }
    
    private bool IsValidCall(string call)
    {
        if (call.Contains('<') || call.Contains('>')) return false;
        if (call.All(char.IsDigit)) return false;
        if (!call.Any(char.IsDigit)) return false;
        return true;
    }
    
    private async Task<DxMapSpot> CreateSpot(WsjtxStatus instance, WsjtxDecode decode)
    {
        _logger.LogDebug("Create Spot");
        
        var spot = new DxMapSpot()
        {
            SpotterCallsign = instance.DeCallsign,
            SpotterGrid = instance.DeGrid,
            DxCallsign = decode.Callsign,
            DxGrid = decode.Exchange,
            Freq = instance.DialFrequency,
            IsCq = decode.IsCq,
            Mode = instance.Mode,
            Snr = decode.Snr,
            Df = decode.DeltaFrequency
            
        };
        
        await SendSpot(spot);
        
        return spot;
    }

    private async Task SendSpot(DxMapSpot spot)
    {
        if (!spot.IsSpotValid)
        {
            _logger.LogError("Invalid spot skipped for DxMaps: {Spot}", spot.CreateSpotMessage());
            return;
        }
        
        if (_config.GetValue<bool>("DxMaps:SendSpot"))
        {
            var buffer = Encoding.ASCII.GetBytes(spot.CreateSpotMessage());
            var sent = await _udpClient.SendAsync(buffer, buffer.Length, _config["DxMaps:Host"], 
                _config.GetValue<int>("DxMaps:Port"));
            _logger.LogInformation("Sent {Payload} of {Sent} bytes to DxMaps", spot.CreateSpotMessage(), sent);
        }
        else
        {
            _logger.LogInformation("DxMaps Spot Disabled: {Payload} not sent", spot.CreateSpotMessage());
        }
    }
}