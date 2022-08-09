using MessagePublisher;
using MessagePublisher.Mqtt;
using MessagePublisher.Service;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<IMqttClient, MqttClient>();
        services.AddSingleton<IMessagePubService, MqttPubService>();
    })
    .Build();

await host.RunAsync();