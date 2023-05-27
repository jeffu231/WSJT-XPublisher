using System.Net;
using M0LTE.WsjtxUdpLib.Messages;

namespace MessagePublisher.Provider;

public interface IWsjtxClient
{
    public Task<bool> SendMessage(IWsjtxCommandMessage msg);
    
    public event EventHandler<WsjtxMessage>? MessageReceived;
}