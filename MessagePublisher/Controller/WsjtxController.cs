using Asp.Versioning;
using MessagePublisher.Attributes;
using MessagePublisher.Models;
using MessagePublisher.Service;
using Microsoft.AspNetCore.Mvc;
using WsjtxClient.Messages.In;
using WsjtxClient.Provider;

namespace MessagePublisher.Controller;

[ApiController]
[Route("api/wsjtx/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class WsjtxController: ControllerBase
{
    private readonly ILogger<WsjtxController> _logger;
    private readonly IWsjtxDataProviderManager _wsjtxDataProviderManager;
    
    public WsjtxController(ILogger<WsjtxController> logger, IWsjtxDataProviderManager wsjtxDataWsjtxDataProviderManagerManager)
    {
        _logger = logger;
        _wsjtxDataProviderManager = wsjtxDataWsjtxDataProviderManagerManager;
    }
    
    [HttpGet("wsjtx/instances")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Instances()
    {
        var instances = new List<string>();
        foreach (var wsjtxDataProvider in _wsjtxDataProviderManager.WsjtxDataProviders)
        {
            foreach (var instance in wsjtxDataProvider.Instances)
            {
                instances.Add(instance);
            }
        }
        
        return await Task.FromResult(Ok(instances));
    }
    
    [HttpGet("wsjtx/{id}/status")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Status(string id)
    {
        var instances = _wsjtxDataProviderManager.WsjtxDataProviders.Where(x => x.Instances.Contains(id)).ToArray();
        if (instances.Any())
        {
            return await Task.FromResult(Ok(instances.First().Status(id)));
        }
        return await Task.FromResult(NotFound("Id not found"));
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
        var guidId = Guid.Parse(id);
        var instance = _wsjtxDataProviderManager.WsjtxDataProviders.Where(x => x.Id.Equals(guidId)).ToArray();
        if (instance.Any())
        {
            return await Task.FromResult(Ok(instance.First().SendMessage(lm)));
        }
        
        return await Task.FromResult(NotFound("Id not found"));
    }
}