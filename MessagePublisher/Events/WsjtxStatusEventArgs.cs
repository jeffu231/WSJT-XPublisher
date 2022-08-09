using MessagePublisher.Models;

namespace MessagePublisher.Events
{
    public class WsjtxStatusEventArgs
    {
        public WsjtxStatusEventArgs(WsjtxStatus status)
        {
            Status = status;
        }

        public WsjtxStatus Status { get; }
    }
}