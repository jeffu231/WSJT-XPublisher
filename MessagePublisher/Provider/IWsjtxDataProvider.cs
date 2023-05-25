using MessagePublisher.Events;

namespace MessagePublisher.Provider;

public interface IWsjtxDataProvider: IHostedService
{ 
    public Guid Id { get; }
    
    public event EventHandler<WsjtxDecodeEventArgs>? DecodeReceived;
        
    public event EventHandler<WsjtxStatusEventArgs>? StatusReceived;
}