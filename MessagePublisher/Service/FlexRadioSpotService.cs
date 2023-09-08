using System.Collections.Concurrent;
using MessagePublisher.Models;
using WsjtxClient.Events;
using WsjtxClient.Models;
using WsjtxClient.Provider;

namespace MessagePublisher.Service;

public class FlexRadioSpotService:BackgroundService
{
    private readonly ILogger<FlexRadioSpotService> _logger;
    private readonly IConfiguration _config;
    private readonly IWsjtxDataProvider _dataProvider;
    private readonly FlexRadioApiService _flexRadioApiService;
    private readonly ConcurrentDictionary<string, WsjtxStatus> _wsjtxInstance;
    private readonly ConcurrentQueue<FlexSpot> _spotQueue;
    private readonly Dictionary<string, FlexSpot> _txSpotList;
    private bool _isEnabled;
    
    private const string TransmitColor = "#FF0000";
    private const string ReceiveColor = "#FFFF00";
    private const string CqColor = "#01FF00";
    private const string TxText = "TX";
    private readonly string _spotterCall;
    
    public FlexRadioSpotService(IWsjtxDataProvider wsjtxDataProvider, IConfiguration configuration, 
        FlexRadioApiService flexRadioApiService, ILogger<FlexRadioSpotService> logger)
    {
        _logger = logger;
        _config = configuration;
        _dataProvider = wsjtxDataProvider;
        _flexRadioApiService = flexRadioApiService;
        _wsjtxInstance = new ConcurrentDictionary<string, WsjtxStatus>();
        _spotQueue = new ConcurrentQueue<FlexSpot>();
        _txSpotList = new Dictionary<string, FlexSpot>();
        _spotterCall = configuration.GetValue<string>("FlexSpot:SpotterCall") ?? "WSJTX";
        _logger.LogDebug("Constructor exit");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("FlexSpot service execute starting");
        await _flexRadioApiService.ClearSpotsAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            IsEnabled = _config.GetValue<bool>("FlexSpot:Enabled");
            if (_spotQueue.Any())
            {
                await SendSpots();
                continue;
            }
            
            await Task.Delay(1000, stoppingToken);    
            
        }
        
        _logger.LogDebug("DxMaps service execute finishing");
    }

    private bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if(_isEnabled == value) return;
            _isEnabled = value;
            if (_isEnabled)
            {
                _dataProvider.DecodeReceived += DataProviderOnDataReceived;
                _dataProvider.StatusReceived += DataProviderOnStatusReceived;
            }
            else
            {
                _dataProvider.DecodeReceived -= DataProviderOnDataReceived;
                _dataProvider.StatusReceived -= DataProviderOnStatusReceived;
            }
            
            _logger.LogInformation("FlexSpots Enabled {Disabled}", _isEnabled);
        }
    }
    
    private void DataProviderOnDataReceived(object? sender, WsjtxDecodeEventArgs e)
    {
        _logger.LogDebug("WSJT-X Data Message {data}", e.Decode);

        var decode = e.Decode;
        if (!decode.LowConfidence  && !decode.Callsign.Contains("<"))
        {
            if (_wsjtxInstance.TryGetValue(decode.Id, out var instance))
            {
                var s = CreateSpot(instance, e.Decode);
                _spotQueue.Enqueue(s);
            }
        }
    }

    private async void DataProviderOnStatusReceived(object? sender, WsjtxStatusEventArgs e)
    {
        _logger.LogDebug("WSJT-X Status Message {Status}", e.Status);
        
        if (_wsjtxInstance.TryGetValue(e.Status.Id, out var status))
        {
            _wsjtxInstance[e.Status.Id] = e.Status;
        }
        else
        {
            _wsjtxInstance.TryAdd(e.Status.Id, e.Status);
        }
        await UpdateOrCreateTransmitSpot(e.Status);
    }

    private async Task UpdateOrCreateTransmitSpot(WsjtxStatus status)
    {
        //The flex radio appears to not support spaces in the source field, so we have to convert those
        //to something else. Using underscore for now, but that may need to change at some point. Here we 
        //compare by removing the spaces to what we did further down creating the spot
        
        var updateSpot = false;
        if (_txSpotList.TryGetValue(status.Id.Replace(" ", "_"), out var oldSpot))
        {
            var f = (double)((status.DialFrequency + (ulong)status.TxDF) * .000001m);
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
            SpotterCallsign = _spotterCall,
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
                _logger.LogInformation("Sent {Count} Spots to the Fex", spots.Count);
            }
        }
    }
}