namespace MessagePublisher.Mqtt;

public interface IMqttClient
{
    Task<bool> Publish(string topic, string message);
}