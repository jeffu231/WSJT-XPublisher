using System.Collections.Concurrent;
using System.Globalization;
using MaidenheadLib;
using MessagePublisher.Events;
using MessagePublisher.Models;
using MessagePublisher.Mqtt;
using MessagePublisher.Provider;

namespace MessagePublisher.Service;

public class MqttPubService:IMessagePubService
{
    private readonly WsjtxDataProvider _dataProvider;
    private readonly ConcurrentDictionary<string, WsjtxStatus> _wsjtxInstance;
    private readonly string _rootTopic;
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttPubService> _logger;

    public MqttPubService(IMqttClient mqttClient, IConfiguration configuration, ILogger<MqttPubService> logger)
    {
        _mqttClient = mqttClient;
        _logger = logger;
        _rootTopic = configuration["Mqtt:RootTopic"];
        var listenIp = configuration["Wsjtx:Listener:Ip"];
        var listenPort = configuration.GetValue<int>("Wsjtx:Listener:Port");
        _wsjtxInstance = new ConcurrentDictionary<string, WsjtxStatus>();
        _dataProvider = new WsjtxDataProvider(listenIp, listenPort);
        _dataProvider.DecodeReceived += DataProviderOnDataReceived;
        _dataProvider.StatusReceived += DataProviderOnStatusReceived;
    }
    
    public void Start()
    {
        _dataProvider.Start();
    }

    public void Stop()
    {
        _dataProvider.Stop();
    }
    
    private void DataProviderOnDataReceived(object? sender, WsjtxDecodeEventArgs e)
    {
        
    }

    private async void DataProviderOnStatusReceived(object? sender, WsjtxStatusEventArgs e)
    {
        _logger.LogDebug("WSJT-X Status Message {Status}", e.Status);
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

    private async Task PublishBearing(string instance, string deGrid, string dxGrid)
    {
        if (deGrid != String.Empty && dxGrid != String.Empty)
        {
            await PublishValue(instance, "bearing", Bearing(deGrid, dxGrid).ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            await PublishValue(instance, "bearing", String.Empty);
        }
    }

    private async Task PublishValue(string instance, string key, string value)
    {
        Console.Out.WriteLine(value);
        await _mqttClient.Publish($"{_rootTopic}/{instance}/status/{key}", value);
    }

    private static double Bearing(string srcGrid, string destGrid)
    {
        var start = MaidenheadLocator.LocatorToLatLng(srcGrid);
        var end = MaidenheadLocator.LocatorToLatLng(destGrid);
        
        return Math.Round(MaidenheadLocator.Azimuth(start, end), 0, MidpointRounding.AwayFromZero);
    }
}