using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;

namespace MessagePublisher.Mqtt;

public class MqttClient : IMqttClient
{
    private readonly IManagedMqttClient _mqttClient;

    public MqttClient(IConfiguration configuration)
    {
        // Creates a new client
        var builder = new MqttClientOptionsBuilder()
            .WithClientId("WSJT-X_Publisher.Client")
            .WithTcpServer(configuration["Mqtt:Broker"],  configuration.GetValue<int>("Mqtt:Port"));

        // Create client options objects
        var options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(60))
            .WithClientOptions(builder.Build())
            .Build();

        // Creates the client object
        _mqttClient = new MqttFactory().CreateManagedMqttClient();

        // Set up handlers
        _mqttClient.ConnectedAsync += MqttClientOnConnectedAsync;
        _mqttClient.DisconnectedAsync += MqttClientOnDisconnectedAsync;
        _mqttClient.ConnectingFailedAsync += MqttClientOnConnectingFailedAsync;

        // Starts a connection with the Broker
        _mqttClient.StartAsync(options).GetAwaiter().GetResult();
    }

    public async Task<bool> Publish(string topic, string message)
    {
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.EnqueueAsync(topic, message, MqttQualityOfServiceLevel.AtMostOnce, true);
            return true;
        }

        return false;
    }

    private Task MqttClientOnConnectingFailedAsync(ConnectingFailedEventArgs arg)
    {
        Console.Out.WriteLine($"Couldn't connect to broker.{arg.Exception}");
        return Task.CompletedTask;
    }

    private Task MqttClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        Console.Out.WriteLine("Successfully disconnected.");
        return Task.CompletedTask;
    }

    private Task MqttClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        Console.Out.WriteLine("Successfully connected.");
        return Task.CompletedTask;
    }
}