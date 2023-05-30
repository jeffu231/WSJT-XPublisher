using System;
using System.Threading.Tasks;
using M0LTE.WsjtxUdpLib.Messages;

namespace M0LTE.WsjtxUdpLib.Provider;

public interface IWsjtxClient
{
    public Task<bool> SendMessage(IWsjtxCommandMessage msg);
    
    public event EventHandler<WsjtxMessage>? MessageReceived;
}