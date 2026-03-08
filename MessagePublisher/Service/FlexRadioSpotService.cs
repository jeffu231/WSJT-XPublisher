using System.Collections.Concurrent;
using MessagePublisher.Models;
using MessagePublisher.Models.Settings;
using Microsoft.Extensions.Options;
using WsjtxClient.Events;
using WsjtxClient.Models;

namespace MessagePublisher.Service;

public class FlexRadioSpotService:BackgroundService
{
    private readonly ILogger<FlexRadioSpotService> _logger;
    private readonly IWsjtxDataProviderManager _dataProviderManager;
    private readonly FlexRadioApiService _flexRadioApiService;
    private readonly ConcurrentDictionary<string, WsjtxStatus> _wsjtxInstance;
    private readonly ConcurrentQueue<FlexSpot> _spotQueue;
    private readonly Dictionary<string, FlexSpot> _txSpotList;
    private readonly IOptions<FlexSpotSettings> _flexSpotSettings;

    private const string TransmitColor = "#FF0000";
    private const string ReceiveColor = "#FFFF00";
    private const string CqColor = "#01FF00";
    private const string TxText = "TX";
    private const string DefaultSpotterCall = "WSJTX";
    
    
    public FlexRadioSpotService(IWsjtxDataProviderManager wsjtxDataProviderManager, FlexRadioApiService flexRadioApiService,
        ILogger<FlexRadioSpotService> logger, IOptions<FlexSpotSettings> flexSpotSettings)
    {
        _logger = logger;
        _flexSpotSettings = flexSpotSettings;
        _dataProviderManager = wsjtxDataProviderManager;
        _flexRadioApiService = flexRadioApiService;
        _wsjtxInstance = new ConcurrentDictionary<string, WsjtxStatus>();
        _spotQueue = new ConcurrentQueue<FlexSpot>();
        _txSpotList = new Dictionary<string, FlexSpot>();
        _logger.LogDebug("Constructor exit");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FlexSpot service starting");
        _logger.LogInformation("Initializing with settings: {Settings}", _flexSpotSettings.Value.ToString());
        await _flexRadioApiService.ClearSpotsAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            IsEnabled = _flexSpotSettings.Value.Enabled;
            if (_spotQueue.Any())
            {
                await SendSpots();
                continue;
            }
            
            await Task.Delay(1000, stoppingToken);    
            
        }
        
        _logger.LogInformation("FlexSpot service finishing");
    }

    private bool IsEnabled
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (field)
            {
                foreach (var wsjtxDataProvider in _dataProviderManager.WsjtxDataProviders)
                {
                    _logger.LogInformation("Subscribing to provider {Id}", wsjtxDataProvider.Id);
                    wsjtxDataProvider.DecodeReceived += DataProviderManagerOnDataReceived;
                    wsjtxDataProvider.StatusReceived += DataProviderManagerOnStatusReceived;
                }
            }
            else
            {
                foreach (var wsjtxDataProvider in _dataProviderManager.WsjtxDataProviders)
                {
                    _logger.LogInformation("UnSubscribing to provider {Id}", wsjtxDataProvider.Id);
                    wsjtxDataProvider.DecodeReceived -= DataProviderManagerOnDataReceived;
                    wsjtxDataProvider.StatusReceived -= DataProviderManagerOnStatusReceived;
                }
            }

            _logger.LogInformation("FlexSpots Enabled: {Disabled}", field);
        }
    }

    private void DataProviderManagerOnDataReceived(object? sender, WsjtxDecodeEventArgs e)
    {
        _logger.LogDebug("WSJT-X Data Message {Decode}", e.Decode);

        var decode = e.Decode;
        if (!decode.LowConfidence && IsValidCall(decode.Callsign))
        {
            if (_wsjtxInstance.TryGetValue(decode.Id, out var instance))
            {
                var s = CreateSpot(instance, e.Decode);
                _spotQueue.Enqueue(s);
            }
        }
    }

    private async void DataProviderManagerOnStatusReceived(object? sender, WsjtxStatusEventArgs e)
    {
        try
        {
            _logger.LogDebug("WSJT-X Status Message {Status}", e.Status);
        
            if (_wsjtxInstance.TryGetValue(e.Status.Id, out _))
            {
                _wsjtxInstance[e.Status.Id] = e.Status;
            }
            else
            {
                _wsjtxInstance.TryAdd(e.Status.Id, e.Status);
            }
            await UpdateOrCreateTransmitSpot(e.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transmit spot");
        }
    }

    private async Task UpdateOrCreateTransmitSpot(WsjtxStatus status)
    {
        //The flex radio appears to not support spaces in the source field, so we have to convert those
        //to something else. Using underscore for now, but that may need to change at some point. Here we 
        //compare by removing the spaces to what we did further down creating the spot
        
        var updateSpot = false;
        if (_txSpotList.TryGetValue(status.Id.Replace(" ", "_"), out var oldSpot))
        {
            var f = (double)((status.DialFrequency + status.TxDF) * .000001m);
            if (Math.Abs(oldSpot.RxFrequency - f) > .00001)
            {
                updateSpot = true;
            }

            if (status.IsTransmitting && oldSpot.Color.Equals(ReceiveColor))
            {
                updateSpot = true;
            }
            else if (!status.IsTransmitting && oldSpot.Color.Equals(TransmitColor))
            {
                updateSpot = true;
            }

            if (DateTime.Now - oldSpot.Timestamp > TimeSpan.FromSeconds(240))
            {
                updateSpot = true;
            }
        }

        if (oldSpot == null || updateSpot)
        {
            //The flex radio appears to not support spaces in the source field, so we have to convert those
            //to something else. Using underscore for now, but that may need to change at some point
            var newSpot = new FlexSpot
            {
                Callsign = TxText,
                RxFrequency = (double)((status.DialFrequency + status.TxDF) * .000001m),
                Mode = status.Mode,
                Timestamp = DateTime.Now,
                Source = status.Id.Replace(" ", "_"),
                TriggerAction = "None",
                Comment = "TX DF",
                Priority = 1,
                BackgroundColor = "#FF0000",
                Color = ReceiveColor,
                LifetimeSeconds = 260
            };
            
            _logger.LogDebug("Adding TX spot to Flex for {NewSpotCall} at {NewSpotRxFrequency}", 
                newSpot.Callsign, newSpot.RxFrequency);
            _txSpotList[newSpot.Source] = newSpot;
            _spotQueue.Enqueue(newSpot);
            
            if (oldSpot != null)
            {
                await RemoveSpot(oldSpot);
            }
        }
    }

    private FlexSpot CreateSpot(WsjtxStatus instance, WsjtxDecode decode)
    {
        _logger.LogDebug("Create Spot");
        
        FlexSpot spot = new()
        {
            Callsign = decode.Callsign,
            RxFrequency = (double) ((instance.DialFrequency + (ulong) decode.DeltaFrequency) * .000001m),
            LifetimeSeconds = 30,
            Mode = instance.Mode,
            Timestamp = DateTime.Today + decode.SinceMidnight + TimeZoneInfo.Local.GetUtcOffset(DateTime.Today),
            Source = decode.Id,
            TriggerAction = "None",
            Comment = decode.Message,
            SpotterCallsign = string.IsNullOrEmpty(_flexSpotSettings.Value.SpotterCall)?DefaultSpotterCall:
                _flexSpotSettings.Value.SpotterCall,
            Priority = 5,
            Color = ReceiveColor
        };

                    
        if (decode.IsCq)
        {
            spot.Color = CqColor;
            spot.Priority = 4;
        }
        
        return spot;
    }
    
    private bool IsValidCall(string call)
    {
        if (call.Contains('<') || call.Contains('>')) return false;
        if (call.All(char.IsDigit)) return false;
        if (!call.Any(char.IsDigit)) return false;
        return true;
    }

    private async Task RemoveSpot(FlexSpot spot)
    {
        if (IsEnabled)
        {
            await _flexRadioApiService.RemoveSpotAsync(spot);
        }
    }

    private async Task SendSpots()
    {
        if (IsEnabled)
        {
            var count = 0;
            var limit = 20;
            var spots = new List<FlexSpot>();
            while (_spotQueue.TryDequeue(out var spot) && count < limit)
            {
                spots.Add(spot);
                count++;
            }

            if (spots.Any())
            {
                await _flexRadioApiService.SendSpotsAsync(spots);
                _logger.LogDebug("Sent {Count} Spots to the Flex", spots.Count);
            }
        }
    }
}