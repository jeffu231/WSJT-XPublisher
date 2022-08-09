using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using M0LTE.WsjtxUdpLib.Messages;

namespace MessagePublisher.Provider
{
    /// <summary>
    /// WSJTX Client class based on the example from M0LTE.WsjtxUdpLib
    /// </summary>
    public sealed class WsjtxClient : IDisposable
    {
        private readonly UdpClient _udpClient;
        private readonly Action<WsjtxMessage, IPEndPoint> callback;
        private readonly ConcurrentDictionary<string, IPEndPoint> _endPoints;
        private readonly IPAddress _ipAddress;
       
        public WsjtxClient(Action<WsjtxMessage, IPEndPoint> callback, IPAddress ipAddress, int port = 2237, bool multicast = false)
        {
            _ipAddress = ipAddress;
            _endPoints = new ConcurrentDictionary<string, IPEndPoint>();
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

            this.callback = callback;
            _ = Task.Run(UdpLoop);
        }
        private void UdpLoop()
        {
            var from = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                byte[] datagram = _udpClient.Receive(ref from);

                WsjtxMessage msg;

                try
                {
                    msg = WsjtxMessage.Parse(datagram);
                    if (msg is CloseMessage cm)
                    {
                        _endPoints.Remove(msg.Id, out _);
                    }
                    else
                    {
                        _endPoints[msg.Id] = from;
                    }
                    
                    //Console.WriteLine($"Type is {msg.GetType()}");
                    
                    // Console.Out.WriteLine($"Message for {msg.Id} received from {from}");
                }
                catch (ParseFailureException ex)
                {
                    Console.WriteLine($"Parse failure for {ex.MessageType}: {ex.Message}");
                    continue;
                }

                callback(msg, @from);
            }
        }

        public async Task<bool> SendMessage(IWsjtxCommandMessageGenerator msg)
        {
            if (_endPoints.TryGetValue(msg.Id, out var endPoint))
            {
                var bytesToSend = msg.GetBytes();
                try
                {
                    await _udpClient.SendAsync(bytesToSend, bytesToSend.Length, endPoint);
                    Console.WriteLine($"Sent highlight msg");
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Send msg failed {e.Message}");
                }
            }
            return false;
        }
        
        public void Dispose()
        {
            _udpClient.Dispose();
        }
    }
}