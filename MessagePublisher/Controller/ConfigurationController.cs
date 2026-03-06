using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace MessagePublisher.Controller;

[ApiController]
[Route("api/wsjtx/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ConfigurationController:ControllerBase
{
    private readonly ILogger<ConfigurationController> _logger;
    private readonly IConfiguration _config;

    public ConfigurationController(ILogger<ConfigurationController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _config = configuration;
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
        return await Task.FromResult(Ok(_config.GetValue<bool>("DxMaps:Enabled")));
    }
    
    /// <summary>
    /// Retrieves the current enabled state of the MQTT feature from the configuration.
    /// </summary>
    /// <returns>Boolean value indicating whether the MQTT feature is enabled.</returns>
    [HttpGet("mqtt/enabled")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> MqttEnabled()
    {
        return await Task.FromResult(Ok(_config.GetValue<bool>("Mqtt:Enabled")));
    }
    
    /// <summary>
    /// Sets the enabled state of the DxMaps feature in the configuration.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Result</returns>
    [HttpPost("dxmaps/enabled/{enable}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> DxMapsEnabled(bool enable)
    {
        return await Task.FromResult(Ok(_config["DxMaps:Enabled"] = enable.ToString()));
    }
    
    /// <summary>
    /// Sets the enabled state of the MQTT feature in the configuration.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Result</returns>
    [HttpPost("mqtt/enabled/{enable}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> MqttEnabled(bool enable)
    {
        return await Task.FromResult(Ok(_config["Mqtt:Enabled"] = enable.ToString()));
    }
}