using M0LTE.WsjtxUdpLib.Messages;
using MessagePublisher.Events;
using MessagePublisher.Models;

namespace MessagePublisher.Provider;

public interface IWsjtxDataProvider: IHostedService
{ 
    public Guid Id { get; }

    /// <summary>
    /// Gets a list of instance ids discovered.
    /// </summary>
    public List<string> Instances { get; }

    /// <summary>
    /// Gets the latest status for a given WSJTX instance that is active or null if not found.
    /// </summary>
    /// <param name="id">Instance id</param>
    /// <returns></returns>
    public WsjtxStatus? Status(string id);

    /// <summary>
    /// Updates the grid locator in the specified instance of WSJTX
    /// </summary>
    /// <param name="msg">The WSJTX Message to send</param>
    /// <returns></returns>
    public Task<bool> SendMessage(IWsjtxCommandMessage msg);

    public event EventHandler<WsjtxDecodeEventArgs>? DecodeReceived;
        
    public event EventHandler<WsjtxStatusEventArgs>? StatusReceived;
}