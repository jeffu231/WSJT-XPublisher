using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using M0LTE.WsjtxUdpLib.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace M0LTE.WsjtxUdpLib.Provider
{
    /// <summary>
    /// WSJTX Client class based on the example from M0LTE.WsjtxUdpLib
    /// </summary>
    public sealed class WsjtxClient : IWsjtxClient, IDisposable
    {
        private readonly UdpClient _udpClient;
        private readonly ConcurrentDictionary<string, IPEndPoint> _endPoints;
        private readonly ILogger<WsjtxClient> _logger;

        public WsjtxClient(ILogger<WsjtxClient> logger, IConfiguration configuration)
        {
            _logger = logger;
            _endPoints = new ConcurrentDictionary<string, IPEndPoint>();
            try
            {
                var ipAddress = IPAddress.Parse(configuration["Wsjtx:Listener:Ip"]??"224.0.0.1");
                var port = configuration.GetValue<int>("Wsjtx:Listener:Port");
                var multicast = configuration.GetValue<bool>("Wsjtx:Listener:Multicast");
                if (multicast)
                {
                    _udpClient = new UdpClient();
                    _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                    _udpClient.JoinMulticastGroup(ipAddress);
                }
                else
                {
                    _udpClient = new UdpClient(new IPEndPoint(ipAddress, port));
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical("Invalid IP Address {Message}", e.Message);
                throw;
            }
            
            _ = Task.Run(UdpLoop);
        }
        
        private void UdpLoop()
        {
            var from = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                byte[] datagram = _udpClient.Receive(ref from);

                try
                {
                    var msg = WsjtxMessage.Parse(datagram);
                    if (msg is CloseMessage cm)
                    {
                        _endPoints.TryRemove(msg.Id, out _);
                    }
                    else
                    {
                        _endPoints[msg.Id] = from;
                    }
                    
                    OnMessageReceived(msg);
                    
                    _logger.LogTrace("Message for {MsgId} received from {From}", msg.Id, from);
                }
                catch (ParseFailureException ex)
                {
                    _logger.LogError("Parse failure for {ExMessageType}: {ExMessage}", ex.MessageType, ex.Message);
                }
            }
        }

        public async Task<bool> SendMessage(IWsjtxCommandMessage msg)
        {
            if (_endPoints.TryGetValue(msg.Id, out var endPoint))
            {
                var bytesToSend = msg.GetBytes();
                try
                {
                    await _udpClient.SendAsync(bytesToSend, bytesToSend.Length, endPoint);
                    _logger.LogTrace("Sent wsjtx msg bytes {Length} to {S}", bytesToSend.Length, endPoint.ToString());
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError("Send wsjtx msg failed {EMessage}", e.Message);
                }
            }

            return false;
        }
        
        public void Dispose()
        {
            _udpClient.Dispose();
        }

        public event EventHandler<WsjtxMessage>? MessageReceived;

        private void OnMessageReceived(WsjtxMessage msg)
        {
            MessageReceived?.Invoke(this, msg);
        }
    }
}