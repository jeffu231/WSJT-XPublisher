using MessagePublisher.Mqtt;
using MessagePublisher.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using WsjtxClient.Provider;

namespace MessagePublisher;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        ConfigureServices(builder.Services);

        ConfigureApiVersioning(builder);
            
        ConfigureSwagger(builder);

        var app = builder.Build();

        EnableSwagger(app);

        app.UseAuthorization();

        app.UseExceptionHandler();
            
        app.UseStatusCodePages();

        app.MapControllers();
            
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IWsjtxClient, WsjtxClient.Provider.WsjtxClient>();
        services.AddSingleton<IMqttClient, MqttClient>();
        services.AddSingleton<IWsjtxDataProvider, WsjtxDataProvider>();
        services.AddHostedService<IWsjtxDataProvider>(provider => provider.GetRequiredService<IWsjtxDataProvider>());
        services.AddHostedService<MqttPubService>();
        services.AddHostedService<DxMapsSpotService>();
        
        services.AddProblemDetails();
        
        services.AddControllers(o =>
        {
            o.RespectBrowserAcceptHeader = true;
            o.ReturnHttpNotAcceptable = true;
        }).AddNewtonsoftJson().AddXmlSerializerFormatters();
    }
    
    private static void ConfigureApiVersioning(WebApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"));
        });
            
        // Add ApiExplorer to discover versions
        builder.Services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });
    }

    private static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
            
        builder.Services.AddSwaggerGen();

        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
    }

    private static void EnableSwagger(WebApplication app)
    {
        var swaggerBasePath = "api/wsjtx";

        app.UseSwagger(options =>
        {
            options.RouteTemplate = swaggerBasePath + "/swagger/{documentName}/swagger.{json|yaml}";
        });
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = $"{swaggerBasePath}/swagger";
            var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
                options.SwaggerEndpoint($"{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
        });
    }
}