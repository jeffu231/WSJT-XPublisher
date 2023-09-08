using MessagePublisher.Mqtt;
using MessagePublisher.Service;
using WsjtxClient.Provider;

namespace MessagePublisher;

public static class ConfigServiceCollectionExtension
{
    public static IServiceCollection ConfigureServicesFromConfig(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSingleton<IWsjtxClient, WsjtxClient.Provider.WsjtxClient>();
        services.AddSingleton<IMqttClient, MqttClient>();
        
        //WSJTX Listeners
        List<Listener> listeners = new List<Listener>();
        config.GetSection("Wsjtx:Listeners").Bind(listeners);

        services.AddTransient<IWsjtxClient, WsjtxClient.Provider.WsjtxClient>();
    
    
        foreach (var listener in listeners)
        {
            Console.Out.WriteLine($"Adding listener on port {listener.Port}");
            services.AddSingleton<IHostedService>(x => new WsjtxDataProvider(
                x.GetRequiredService<ILogger<WsjtxDataProvider>>(),
                x.GetRequiredService<IWsjtxClient>(),
                listener));
        }

        services.AddSingleton<IWsjtxDataProviderManager, WsjtxDataProviderManager>();
        services.AddHostedService<MqttPubService>();
        services.AddHostedService<DxMapsSpotService>();
        services.AddHostedService<FlexRadioSpotService>();
        
        services.AddProblemDetails();
        
        services.AddControllers(o =>
        {
            o.RespectBrowserAcceptHeader = true;
            o.ReturnHttpNotAcceptable = true;
        }).AddNewtonsoftJson().AddXmlSerializerFormatters();
        
       
    
    

        return services;
    }
}