namespace M0LTE.WsjtxUdpLib.Messages
{
    public interface IWsjtxCommandMessage: IWsjtxMessage
    {
        byte[] GetBytes();
    }
}
