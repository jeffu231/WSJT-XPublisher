using M0LTE.WsjtxUdpLib.Messages.Out;

namespace MessagePublisher.Models
{
    public class WsjtxStatus
    {
        public static WsjtxStatus DecodeMessage(StatusMessage msg)
        {
            var status = new WsjtxStatus();
            status.ParseStatus(msg);
            return status;
        }
        
        public string Id { get; protected set; } = String.Empty;
        
        public string ConfigurationName { get; set; } = String.Empty;

        /// <summary>
        /// Operating mode
        /// </summary>
        public string Mode { get; set; } = String.Empty;

        public bool IsTransmitting { get; set; }
        
        public bool IsTxEnabled { get; set; }

        public bool IsDecoding { get; set; }

        public string DeCallsign { get; set; } = String.Empty;
        
        public string DxCallsign { get; set; } = String.Empty;
        
        public string DxGrid { get; set; } = String.Empty;
        
        public string DeGrid { get; set; } = String.Empty;
        
        /// <summary>
        ///  Radio Frequency
        /// </summary>
        public ulong DialFrequency { get; set; }

        public uint? TrPeriod { get; set; }

        public uint TxDF { get; set; }
        
        public uint RxDF { get; set; }

        public override string ToString()
        {
            return $"Id:{Id} Config Name:{ConfigurationName} Mode:{Mode} DeCall:{DeCallsign} DeGrid:{DeGrid} " +
                   $"DxCall:{DxCallsign} DxGrid:{DxGrid} " +
                   $"IsTansmitting:{IsTransmitting} IsDecoding:{IsDecoding} " +
                   $"TRPeriod:{TrPeriod} TxDF:{TxDF} RxDF:{RxDF}";
        }
        
        protected void ParseStatus(StatusMessage msg)
        {
            Id =  msg.Id;
            ConfigurationName = msg.ConfigurationName;
            Mode = msg.Mode;
            DeCallsign = msg.DeCall;
            DxCallsign = msg.DxCall;
            DxGrid = msg.DxGrid;
            IsDecoding = msg.Decoding;
            IsTransmitting = msg.Transmitting;
            IsTxEnabled = msg.TxEnabled;
            DeGrid = msg.DeGrid;
            DialFrequency = msg.DialFrequency;
            TrPeriod = msg.TRPeriod;
            TxDF = msg.TxDF;
            RxDF = msg.RxDF;
        }
    }
}