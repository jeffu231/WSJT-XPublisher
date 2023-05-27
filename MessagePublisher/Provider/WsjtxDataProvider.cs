using System.Collections.Concurrent;
using M0LTE.WsjtxUdpLib.Messages;
using M0LTE.WsjtxUdpLib.Messages.Out;
using MessagePublisher.Events;
using MessagePublisher.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MessagePublisher.Provider
{
    public class WsjtxDataProvider: BackgroundService, IWsjtxDataProvider
    {
        private readonly ILogger<WsjtxDataProvider> _logger;
        private readonly ConcurrentDictionary<string, WsjtxStatus> _wsjtxStatus;
        private readonly Dictionary<string, DateTime> _activeInstances;

        private IWsjtxClient _wsjtxClient;

        public WsjtxDataProvider(ILogger<WsjtxDataProvider> logger, IWsjtxClient wsjtxClient)
        {
            _logger = logger;
            _wsjtxClient = wsjtxClient;
            _activeInstances = new Dictionary<string, DateTime>();
            _wsjtxStatus = new ConcurrentDictionary<string, WsjtxStatus>();
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _wsjtxClient.MessageReceived += WsjtxClientOnMessageReceived;
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                foreach (var activeInstance in _activeInstances.ToList())
                {
                    if (now - activeInstance.Value > TimeSpan.FromMinutes(2))
                    {
                        _logger.LogInformation("Removing {Id} not heard last 2 minutes", activeInstance.Key);
                        _activeInstances.Remove(activeInstance.Key);
                        _wsjtxStatus.Remove(activeInstance.Key, out _);
                    }
                }
                await Task.Delay(30000, stoppingToken);
            }
            
        }

        private void WsjtxClientOnMessageReceived(object? sender, WsjtxMessage msg)
        {
            if (msg is DecodeMessage dm)
            {
                _logger.LogTrace("Decode for {Id}", dm.Id);
                ParseDecodeMessage(dm);
            }
            else if (msg is StatusMessage sm)
            {
                _logger.LogTrace("Status for {Id}", sm.Id);
                ParseStatusMessage(sm);
            }
            else if (msg is HeartbeatMessage hm)
            {
                _logger.LogTrace("Heartbeat for {Id}", hm.Id);
                ParseHeartbeatMessage(hm);
            }
        }

        private void ParseStatusMessage(StatusMessage msg)
        {
            var status = WsjtxStatus.DecodeMessage(msg);
            if (_wsjtxStatus.ContainsKey(status.Id))
            {
                _wsjtxStatus[status.Id] = status;
            }
            else
            {
                _wsjtxStatus.TryAdd(status.Id, status);
            }
            
            OnStatusReceived(status);
        }

        private void ParseDecodeMessage(DecodeMessage msg)
        {
            var decode = WsjtxDecode.DecodeMessage(msg);
            OnDecodeReceived(decode);
        }

        private void ParseHeartbeatMessage(HeartbeatMessage msg)
        {
            _logger.LogInformation("{Beat}", msg.ToString() );
            if (_activeInstances.ContainsKey(msg.Id))
            {
                _activeInstances[msg.Id] = DateTime.Now;
            }
            else
            {
                _activeInstances.TryAdd(msg.Id, DateTime.Now);
            }
        }

        public Guid Id { get; } = Guid.NewGuid();
        
        public List<string> Instances => _wsjtxStatus.Keys.ToList();

        public WsjtxStatus? Status(string id)
        {
            if (_wsjtxStatus.TryGetValue(id, out var status))
            {
                return status;
            }

            return null;
        }

        public async Task<bool> SendMessage(IWsjtxCommandMessage msg)
        {
            return await _wsjtxClient.SendMessage(msg);
        }

        public event EventHandler<WsjtxDecodeEventArgs>? DecodeReceived;
        
        public event EventHandler<WsjtxStatusEventArgs>? StatusReceived;

        private void OnDecodeReceived(WsjtxDecode decode)
        {
            DecodeReceived?.Invoke(this,new WsjtxDecodeEventArgs(decode));
        }
        
        private void OnStatusReceived(WsjtxStatus status)
        {
            StatusReceived?.Invoke(this,new WsjtxStatusEventArgs(status));
        }
        
    }
}