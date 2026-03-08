using System.Reflection;
using Asp.Versioning;
using MessagePublisher.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MessagePublisher.Controller;

[ApiController]
[Route("api/wsjtx/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ConfigurationController:ControllerBase
{
    private readonly ILogger<ConfigurationController> _logger;
    private readonly IOptions<FlexSpotSettings> _flexSpotSettings;
    private readonly IOptions<MqttBrokerSettings> _mqttBrokerSettings;
    private readonly IOptions<DxMapsSettings> _dxMapsSettings;

    public ConfigurationController(ILogger<ConfigurationController> logger, IOptions<FlexSpotSettings> flexSpotSettings, 
        IOptions<MqttBrokerSettings> mqttBrokerSettings, IOptions<DxMapsSettings> dxMapsSettings)
    {
        _logger = logger;
        _flexSpotSettings = flexSpotSettings;
        _mqttBrokerSettings = mqttBrokerSettings;
        _dxMapsSettings = dxMapsSettings;
    }
    
    /// <summary>
    /// Get the version of the application
    /// </summary>
    /// <returns>Application Version</returns>
    [HttpGet("version")]
    [MapToApiVersion("1.0")]
    public IActionResult GetVersion()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        return Ok(new { ApplicationVersion = version });
    }

    /// <summary>
    /// Retrieves the current enabled state of the DxMaps feature from the configuration.
    /// </summary>
    /// <returns>Boolean value indicating whether the DxMaps feature is enabled.</returns>
    [HttpGet("dxmaps/enabled")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> DxMapsEnabled()
    {
        return await Task.FromResult(Ok(_dxMapsSettings.Value.Enabled));
    }
    
    /// <summary>
    /// Retrieves the current enabled state of the MQTT feature from the configuration.
    /// </summary>
    /// <returns>Boolean value indicating whether the MQTT feature is enabled.</returns>
    [HttpGet("mqtt/enabled")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> MqttEnabled()
    {
        return await Task.FromResult(Ok(_mqttBrokerSettings.Value.Enabled));
    }
    
    /// <summary>
    /// Retrieves the current enabled state of the Flex Spot feature from the configuration.
    /// </summary>
    /// <returns>Boolean value indicating whether the Flex Spot feature is enabled.</returns>
    [HttpGet("flexspot/enabled")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> FlexSpotEnabled()
    {
        return await Task.FromResult(Ok(_flexSpotSettings.Value.Enabled));
    }
    
    /// <summary>
    /// Sets the enabled state of the DxMaps feature in the configuration.
    /// This is not persisted across restarts.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Result</returns>
    [HttpPost("dxmaps/enabled/{enable}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> DxMapsEnabled(bool enable)
    {
        _dxMapsSettings.Value.Enabled = enable;
        return await Task.FromResult(Ok(_dxMapsSettings.Value.Enabled));
    }
    
    /// <summary>
    /// Sets the enabled state of the MQTT feature in the configuration.
    /// This is not persisted across restarts.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Result</returns>
    [HttpPost("mqtt/enabled/{enable}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> MqttEnabled(bool enable)
    {
        _mqttBrokerSettings.Value.Enabled = enable;
        return await Task.FromResult(Ok(_mqttBrokerSettings.Value.Enabled));
    }
    
    /// <summary>
    /// Sets the enabled state of the Flex Spot feature in the configuration.
    /// This is not persisted across restarts.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Result</returns>
    [HttpPost("flexspot/enabled/{enable}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> FlexSpotEnabled(bool enable)
    {
        _flexSpotSettings.Value.Enabled = enable;
        return await Task.FromResult(Ok(_flexSpotSettings.Value.Enabled));
    }
}