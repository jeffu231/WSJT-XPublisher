using M0LTE.WsjtxUdpLib.Messages;
using M0LTE.WsjtxUdpLib.Provider;
using MessagePublisher.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace MessagePublisher.Controller;

[ApiController]
[Route("api/wsjtx/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class WsjtxController: ControllerBase
{
    private readonly ILogger<WsjtxController> _logger;
    private readonly IWsjtxDataProvider _provider;
    
    public WsjtxController(ILogger<WsjtxController> logger, IWsjtxDataProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }
    
    [HttpGet("wsjtx/instances")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Instances()
    {
        return await Task.FromResult(Ok(_provider.Instances));
    }
    
    [HttpGet("wsjtx/{id}/status")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Status(string id)
    {
        return await Task.FromResult(Ok(_provider.Status(id)));
    }
    
    [HttpPost("wsjtx/{id}/locator")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Locator(string id, [FromBody] [Grid] string locator )
    {
        var lm = new LocationMessage()
        {
            Id = id,
            Locator = locator
        };
        return await Task.FromResult(Ok(_provider.SendMessage(lm)));
    }
}