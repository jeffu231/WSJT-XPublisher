using System.Net;
using M0LTE.WsjtxUdpLib.Messages;
using M0LTE.WsjtxUdpLib.Messages.Out;
using MessagePublisher.Events;
using MessagePublisher.Models;

namespace MessagePublisher.Provider
{
    public class WsjtxDataProvider: BackgroundService, IWsjtxDataProvider
    {
        private readonly int _port;
        private readonly IPAddress _ipAddress;
        private readonly ILogger<WsjtxDataProvider> _logger;

        private WsjtxClient? _wsjtxClient;

        public WsjtxDataProvider(IConfiguration configuration, ILogger<WsjtxDataProvider> logger)
        {
            _logger = logger;
            
            try
            {
                _ipAddress = IPAddress.Parse(configuration["Wsjtx:Listener:Ip"]);
            }
            catch (Exception e)
            {
                _logger.LogCritical("Invalid IP Address {Message}", e.Message);
                throw;
            }
            
            _port = configuration.GetValue<int>("Wsjtx:Listener:Port");
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Start();
            _logger.LogDebug("{DataProviderId}", Id.ToString());
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
            
        }

        private void ClientCallback(WsjtxMessage msg, IPEndPoint endPoint)
        {
            
            if (msg is DecodeMessage dm)
            {
                _logger.LogDebug("Client Callback Decode");
                ParseDecodeMessage(dm);
            }
            else if (msg is StatusMessage sm)
            {
                //_logger.LogDebug("Client Callback Status");
                ParseStatusMessage(sm);
            }
            else if (msg is HeartbeatMessage hm)
            {
                
            }
        }

        private void Start()
        {
            if (_wsjtxClient != null)
            {
                _wsjtxClient.Dispose();
            }
            _wsjtxClient = new WsjtxClient(ClientCallback, _ipAddress, _port, true);
            _logger.LogDebug("Wsjtx Client created");
        }

        public void Stop()
        {
            _wsjtxClient?.Dispose();
        }
        
        private void ParseStatusMessage(StatusMessage msg)
        {
            var status = WsjtxStatus.DecodeMessage(msg);
            OnStatusReceived(status);
        }

        private void ParseDecodeMessage(DecodeMessage msg)
        {
            var decode = WsjtxDecode.DecodeMessage(msg);
            OnDecodeReceived(decode);
            
        }

        public Guid Id { get; } = Guid.NewGuid();
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