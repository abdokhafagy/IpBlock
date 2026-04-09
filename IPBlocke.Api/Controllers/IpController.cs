using IPBlocker.Application.DTOs;
using IPBlocker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IPBlocke.Api.Controllers;

[ApiController]
[Route("api/ip")]
[Produces("application/json")]
public class IpController : ControllerBase
{
    private readonly IIpService _ipService;

    public IpController(IIpService ipService)
    {
        _ipService = ipService;
    }

    /// <summary>
    /// Look up geolocation info for an IP address. 
    /// If ipAddress is omitted, the caller's IP is used.
    /// </summary>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(IpLookupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Lookup([FromQuery] string? ipAddress)
    {
        var callerIp = GetCallerIp();
        var result = await _ipService.LookupIpAsync(ipAddress, callerIp);
        return Ok(result);
    }

    /// <summary>
    /// Check if the caller's IP is from a blocked country. Logs the attempt.
    /// </summary>
    [HttpGet("check-block")]
    [ProducesResponseType(typeof(IpBlockCheckResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckBlock()
    {
        var callerIp = GetCallerIp();
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _ipService.CheckBlockAsync(callerIp, userAgent);
        return Ok(result);
    }

    private string GetCallerIp()
    {
        // Check X-Forwarded-For first (proxy/load-balancer scenario)
        var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            // Take the first IP in the chain (original client)
            return forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
    }
}
