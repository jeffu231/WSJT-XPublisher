namespace MessagePublisher.Models.Settings;

public class MqttBrokerSettings
{
    public string BrokerHost { get; set; } = String.Empty;

    public int BrokerPort { get; set; }

    public string ClientId { get; set; } = String.Empty;

    public string ClientUser { get; set; } = String.Empty;

    public string ClientPassword { get; set; } = String.Empty;
    
    public string RootTopic { get; set; } = String.Empty;
    
    public bool Enabled { get; set; }
    
    public override string ToString()
    {
        return $"MQTT Broker Settings BrokerHost:{BrokerHost} - BrokerPort:{BrokerPort} - ClientId:{ClientId} - Enabled:{Enabled}";
    }
}