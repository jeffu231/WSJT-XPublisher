using System.Net;
using M0LTE.WsjtxUdpLib.Messages;
using M0LTE.WsjtxUdpLib.Messages.Out;
using MessagePublisher.Events;
using MessagePublisher.Models;

namespace MessagePublisher.Provider
{
    public class WsjtxDataProvider
    {
        private readonly int _port;
        private readonly IPAddress _ipAddress;
       
        private WsjtxClient? _wsjtxClient;
        public WsjtxDataProvider(string ipAddress, int port)
        {
            _port = port;
            
            try
            {
                _ipAddress = IPAddress.Parse(ipAddress);
            }
            catch (Exception e)
            {
                Console.Out.Write($"Invalid IP Address {e.Message}");
                throw;
            }
        }

        private void ClientCallback(WsjtxMessage msg, IPEndPoint endPoint)
        {
            if (msg is DecodeMessage dm)
            {
                ParseDecodeMessage(dm);
            }
            else if (msg is StatusMessage sm)
            {
                ParseStatusMessage(sm);
            }
            else if (msg is HeartbeatMessage hm)
            {
                
            }
        }

        public void Start()
        {
            if (_wsjtxClient != null)
            {
                _wsjtxClient.Dispose();
            }
            _wsjtxClient = new WsjtxClient(ClientCallback, _ipAddress, _port, true);
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