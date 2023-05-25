using MessagePublisher;
using MessagePublisher.Mqtt;
using MessagePublisher.Provider;
using MessagePublisher.Service;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IMqttClient, MqttClient>();
        services.AddSingleton<IWsjtxDataProvider, WsjtxDataProvider>();
        services.AddHostedService<IWsjtxDataProvider>(provider => provider.GetRequiredService<IWsjtxDataProvider>());
        services.AddHostedService<MqttPubService>();
        services.AddHostedService<DxMapsSpotPubService>();
    })
    .Build();

await host.RunAsync();