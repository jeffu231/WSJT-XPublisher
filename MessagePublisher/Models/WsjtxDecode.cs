using M0LTE.WsjtxUdpLib.Messages.Out;

namespace MessagePublisher.Models
{
    public class WsjtxDecode
    {
        public static WsjtxDecode DecodeMessage(DecodeMessage msg)
        {
            var decode = new WsjtxDecode();
            decode.ParseDecode(msg);
            return decode;
        }

        /// <summary>
        /// Wsjtx instance Id
        /// </summary>
        public string Id { get; protected set; } = String.Empty;
        
        /// <summary>
        /// Callsign of the working station
        /// </summary>
        public string Callsign { get; protected set; } = string.Empty;
        
        /// <summary>
        /// Callsign of the contact station
        /// </summary>
        public string ContactCallsign { get; protected set; } = string.Empty;

        public bool IsCq { get; protected set; }
        
        /// <summary>
        /// Exchange of the decode if it was present
        /// </summary>
        public string Exchange { get; protected set; } = String.Empty;

        public string Mode { get; set; } = String.Empty;

        public int DeltaFrequency { get; protected set; }

        public TimeSpan SinceMidnight { get; protected set; }

        public int Snr { get; set; }

        public bool LowConfidence { get; set; }

        /// <summary>
        /// Delta Time of the decode
        /// </summary>
        public double DeltaTime { get; protected set; }

        public string Message { get; protected set; } = String.Empty;

        public bool IsExchangeGrid
        {
            get
            {
                if (Exchange.Contains('-') ||
                    Exchange.Contains("73", StringComparison.InvariantCultureIgnoreCase) ||
                    Exchange.Contains("RRR", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                return true;
            }
        }
        
        public override string ToString()
        {
            return
                $"Id:{Id} Mode:{Mode} Callsign:{Callsign} Message: {Message} Exchange: {Exchange} SinceMidnight:{SinceMidnight} DeltaFreq:{DeltaFrequency}";
        }

        protected void ParseDecode(DecodeMessage decode)
        {
            Id = decode.Id;
            Message = decode.Message;
            Mode = decode.Mode;
            DeltaFrequency = decode.DeltaFrequency;
            SinceMidnight = decode.SinceMidnight;
            DeltaTime = decode.DeltaTime;
            Snr = decode.Snr;
            LowConfidence = decode.LowConfidence;
            
            var msg = decode.Message.Trim();
            if(msg.Contains("?"))
            {
                //Flag any ? modifiers and strip them off.
                msg = msg.Substring(0, msg.IndexOf("?", StringComparison.CurrentCulture)).Trim();
            }
            else if(msg.Contains("a"))
            {
                //Flag any a1,a2,etc modifiers and strip them off the end of the message.
                //Sometimes these come alone without the ? if they are not low confidence.
                msg = msg.Substring(0, msg.IndexOf("a", StringComparison.CurrentCulture)).Trim();
            }
            if (msg.StartsWith("CQ"))
            {
                IsCq = true;
            }
            
            var fields = msg.Split(" ");
            if (fields.Length >= 3)
            {
                
                if (!IsCq)
                {
                    Callsign = fields[^2];
                    ContactCallsign = fields[^3];
                }
                else
                {
                    Callsign = fields[^2];
                }
            }
            
            Exchange = fields[^1];
        }
    }
}