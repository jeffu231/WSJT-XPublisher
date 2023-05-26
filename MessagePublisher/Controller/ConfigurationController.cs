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
    
    [HttpGet("dxmaps/enabled")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> DxMapsEnabled()
    {
        return await Task.FromResult(Ok(_config.GetValue<bool>("DxMaps:Enabled")));
    }
    
    [HttpGet("mqtt/enabled")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> MqttEnabled()
    {
        return await Task.FromResult(Ok(_config.GetValue<bool>("Mqtt:Enabled")));
    }
    
    [HttpPost("dxmaps/enabled/{enable}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> DxMapsEnabled(bool enable)
    {
        return await Task.FromResult(Ok(_config["DxMaps:Enabled"] = enable.ToString()));
    }
    
    [HttpPost("mqtt/enabled/{enable}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> MqttEnabled(bool enable)
    {
        return await Task.FromResult(Ok(_config["Mqtt:Enabled"] = enable.ToString()));
    }
}