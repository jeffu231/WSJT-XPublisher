using Asp.Versioning;
using MessagePublisher.Attributes;
using MessagePublisher.Service;
using Microsoft.AspNetCore.Mvc;
using WsjtxClient.Messages.In;

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

    /// <summary>
    /// Retrieves a list of all available WSJTX instances managed by the service.
    /// </summary>
    /// <returns>
    /// An IActionResult containing an HTTP 200 response with a list of instance identifiers
    /// if instances are available; otherwise, an error response if no instances are retrieved.
    /// </returns>
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
    
    /// <summary>
    /// Retrieves the status of a specific WSJTX instance identified by the provided identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>
    /// An IActionResult containing an HTTP 200 response with the status of the specified instance
    /// if instances are available; otherwise, an error response if no instances are retrieved.
    /// </returns>
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

    /// <summary>
    /// Updates the locator grid for a specific WSJTX instance.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the WSJTX instance for which the locator is being updated.
    /// </param>
    /// <param name="locator">
    /// A valid grid locator string, formatted as either 4 or 6 characters (e.g., "EM58" or "EM58AR").
    /// </param>
    /// <returns>
    /// An IActionResult containing an HTTP 200 response with the result of the update operation
    /// if the instance is found and updated successfully; otherwise, an HTTP 404 response if
    /// the specified instance is not found.
    /// </returns>
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