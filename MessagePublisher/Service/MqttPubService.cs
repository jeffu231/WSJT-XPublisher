using System.Collections.Concurrent;
using System.Globalization;
using MaidenheadLib;
using MessagePublisher.Models;
using MessagePublisher.Mqtt;
using System.Text.Json;
using WsjtxClient.Events;
using WsjtxClient.Models;

namespace MessagePublisher.Service;

public class MqttPubService:BackgroundService
{
    private readonly IWsjtxDataProviderManager _dataProviderManager;
    private readonly ConcurrentDictionary<string, WsjtxStatus> _wsjtxInstance;
    private readonly string _rootTopic;
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttPubService> _logger;
    private readonly IConfiguration _config;
    private bool _isEnabled;

    public MqttPubService(IMqttClient mqttClient, IWsjtxDataProviderManager wsjtxDataProviderManager, IConfiguration configuration, ILogger<MqttPubService> logger)
    {
        _mqttClient = mqttClient;
        _dataProviderManager = wsjtxDataProviderManager;
        _logger = logger;
        _config = configuration;
        _rootTopic = configuration["Mqtt:RootTopic"]?? string.Empty;
        _wsjtxInstance = new ConcurrentDictionary<string, WsjtxStatus>();
        _logger.LogDebug("MQTT Pub service ctr");

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
                    wsjtxDataProvider.DecodeReceived += DataProviderOnDataReceived;
                    wsjtxDataProvider.StatusReceived += DataProviderOnStatusReceived;
                }
                
            }
            else
            {
                foreach (var wsjtxDataProvider in _dataProviderManager.WsjtxDataProviders)
                {
                    _logger.LogDebug("UnSubscribing to provider {Id}", wsjtxDataProvider.Id);
                    wsjtxDataProvider.DecodeReceived -= DataProviderOnDataReceived;
                    wsjtxDataProvider.StatusReceived -= DataProviderOnStatusReceived;
                }
            }
            
            _logger.LogInformation("Mqtt Enabled {Disabled}", _isEnabled);
        }
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("MQTT Pub service Execute");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            IsEnabled = _config.GetValue<bool>("Mqtt:Enabled");
            await Task.Delay(1000, stoppingToken);
        }
        _logger.LogDebug("MQTT service execute finishing");
    }
    
    private void DataProviderOnDataReceived(object? sender, WsjtxDecodeEventArgs e)
    {
        //TODO determine what to publish for decode events
    }

    private async void DataProviderOnStatusReceived(object? sender, WsjtxStatusEventArgs e)
    {
        //_logger.LogDebug("WSJT-X Status Message {Status}", e.Status);
        await PublishStatus(e.Status);
        if (_wsjtxInstance.ContainsKey(e.Status.Id))
        {
            _wsjtxInstance[e.Status.Id] = e.Status;
        }
        else
        {
            _wsjtxInstance.TryAdd(e.Status.Id, e.Status);
        }
    }

    private async Task PublishStatus(WsjtxStatus status)
    {
        var instance = status.Id;
        var instanceParts = status.Id.Split(" - ");
        if (instanceParts.Length == 2)
        {
            instance = instanceParts[1];
        }
        if(_wsjtxInstance.TryGetValue(status.Id, out var previousStatus))
        {
            _logger.LogDebug("Found previous status for {StatusId}", status.Id);
            if (status.DxCallsign != previousStatus.DxCallsign ||
                status.DxGrid != previousStatus.DxGrid)
            {
                await PublishQsoInfo(status, instance);
            }
            
            if (status.DxCallsign != previousStatus.DxCallsign)
            {
                await PublishValue(instance, "dx_call", status.DxCallsign);
            }
            
            if (status.DxGrid != previousStatus.DxGrid)
            {
                await PublishValue(instance, "dx_grid", status.DxGrid);
                await PublishBearing(instance, status.DeGrid, status.DxGrid);
            }
            
            if (status.DeCallsign != previousStatus.DeCallsign)
            {
                await PublishValue(instance, "de_call", status.DeCallsign);
            }
            
            if (status.DeGrid != previousStatus.DeGrid)
            {
                await PublishValue(instance, "de_grid", status.DeGrid);
                await PublishBearing(instance, status.DeGrid, status.DxGrid);
            }
            
            if (status.Mode != previousStatus.Mode)
            {
                await PublishValue(instance, "mode", status.Mode);
            }
            
            if (status.IsTransmitting != previousStatus.IsTransmitting)
            {
                await PublishValue(instance, "is_transmitting", status.IsTransmitting.ToString());
            }
            
            if (status.IsTxEnabled != previousStatus.IsTxEnabled)
            {
                await PublishValue(instance, "is_tx_enabled", status.IsTxEnabled.ToString());
            }
            
            if (status.DialFrequency != previousStatus.DialFrequency)
            {
                await PublishValue(instance, "dial_frequency", status.DialFrequency.ToString());
            }
            
        }
        else
        {
            _logger.LogInformation("Did not find previous status for {StatusId}", status.Id);
            await PublishQsoInfo(status, instance);
            await PublishValue(instance, "dx_call", status.DxCallsign);
            await PublishValue(instance, "dx_grid", status.DxGrid);
            await PublishValue(instance, "de_call", status.DeCallsign);
            await PublishValue(instance, "de_grid", status.DeGrid);
            await PublishValue(instance, "mode", status.Mode);
            await PublishValue(instance, "is_transmitting", status.IsTransmitting.ToString());
            await PublishValue(instance, "is_tx_enabled", status.IsTxEnabled.ToString());
            await PublishValue(instance, "dial_frequency", status.DialFrequency.ToString());
            await PublishBearing(instance, status.DeGrid, status.DxGrid);
        }
        
        
    }

    private async Task PublishQsoInfo(WsjtxStatus status, string instance)
    {
        _logger.LogInformation("QsoInfo srcGrid: '{SrcGrid}' destGrid: '{DestGrid}'", status.DeGrid, status.DxGrid);
        var bearing = -1d;
        if (!string.IsNullOrEmpty(status.DeGrid) && !string.IsNullOrEmpty(status.DxGrid))
        {
            bearing = Bearing(status.DeGrid, status.DxGrid);
        }
        var dxStation = new QsoInfo(status.DxCallsign, status.DxGrid,
            bearing, status.DeCallsign, status.DeGrid);

        var json = JsonSerializer.Serialize(dxStation, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await PublishValue(instance, "qso_info", json);
    }

    private async Task PublishBearing(string instance, string deGrid, string dxGrid)
    {
        if(string.IsNullOrEmpty(deGrid) || string.IsNullOrEmpty(dxGrid))
        {
            await PublishValue(instance, "bearing", String.Empty);
        }
        else
        {
            await PublishValue(instance, "bearing", Bearing(deGrid.Trim(), dxGrid.Trim()).ToString(CultureInfo.InvariantCulture));
        }
    }

    private async Task PublishValue(string instance, string key, string value)
    {
        _logger.LogDebug("Publish to:{RootTopic}/{Instance}/status/{Key} {Value}", _rootTopic, instance, key, value);
        await _mqttClient.Publish($"{_rootTopic}/{instance}/status/{key}", value);
    }

    private double Bearing(string srcGrid, string destGrid)
    {
        _logger.LogInformation("srcGrid: '{SrcGrid}' destGrid: '{DestGrid}'", srcGrid, destGrid);
        var start = MaidenheadLocator.LocatorToLatLng(srcGrid);
        var end = MaidenheadLocator.LocatorToLatLng(destGrid);
        
        return Math.Round(MaidenheadLocator.Azimuth(start, end), 0, MidpointRounding.AwayFromZero);
    }

    
}