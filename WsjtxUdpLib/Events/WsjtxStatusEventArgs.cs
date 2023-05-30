using M0LTE.WsjtxUdpLib.Models;

namespace M0LTE.WsjtxUdpLib.Events
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