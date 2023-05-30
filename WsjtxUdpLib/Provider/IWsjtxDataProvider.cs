using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using M0LTE.WsjtxUdpLib.Events;
using M0LTE.WsjtxUdpLib.Messages;
using M0LTE.WsjtxUdpLib.Models;
using Microsoft.Extensions.Hosting;

namespace M0LTE.WsjtxUdpLib.Provider;

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
    /// Sends a message to the specified instance of WSJTX
    /// </summary>
    /// <param name="msg">The WSJTX Message to send</param>
    /// <returns></returns>
    public Task<bool> SendMessage(IWsjtxCommandMessage msg);

    public event EventHandler<WsjtxDecodeEventArgs>? DecodeReceived;
        
    public event EventHandler<WsjtxStatusEventArgs>? StatusReceived;
}