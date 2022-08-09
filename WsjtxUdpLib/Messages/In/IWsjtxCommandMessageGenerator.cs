namespace M0LTE.WsjtxUdpLib.Messages
{
    public interface IWsjtxCommandMessageGenerator
    {
        string Id { get; set; }
        byte[] GetBytes();
    }
}
