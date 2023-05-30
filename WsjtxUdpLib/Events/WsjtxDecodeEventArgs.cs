using System;
using M0LTE.WsjtxUdpLib.Models;

namespace M0LTE.WsjtxUdpLib.Events
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