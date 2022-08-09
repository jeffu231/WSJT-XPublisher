using MessagePublisher.Models;

namespace MessagePublisher.Events
{
    public class WsjtxDecodeEventArgs:EventArgs
    {
        public WsjtxDecodeEventArgs(WsjtxDecode decode)
        {
            Decode = decode;
        }

        public WsjtxDecode Decode { get; }
    }
}