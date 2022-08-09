using M0LTE.WsjtxUdpLib.Messages.Out;
using System;

namespace M0LTE.WsjtxUdpLib.Messages
{
    /*
     * Close         Out/In    6                      quint32
     *                         Id (unique key)        utf8
     *
     *      Close is  sent by  a client immediately  prior to  it shutting
     *      down gracefully. When sent by  a server it requests the target
     *      client to close down gracefully.
     */

    public class CloseMessage : WsjtxMessage, IWsjtxCommandMessageGenerator
    {
        public new static WsjtxMessage Parse(byte[] message)
        {
            if (!CheckMagicNumber(message))
            {
                return null;
            }
            int cur = MAGIC_NUMBER_LENGTH;
            var closeMessage = new CloseMessage();
            closeMessage.Id = DecodeString(message, ref cur);
            return closeMessage;
        }

        public byte[] GetBytes() => throw new NotImplementedException();
    }
}
