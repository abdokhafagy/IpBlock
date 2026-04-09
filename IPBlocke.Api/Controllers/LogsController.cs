using IPBlocker.Application.DTOs;
using IPBlocker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IPBlocke.Api.Controllers;

[ApiController]
[Route("api/logs")]
[Produces("application/json")]
public class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Get a paginated list of all block-check attempt logs.
    /// </summary>
    [HttpGet("blocked-attempts")]
    [ProducesResponseType(typeof(PaginatedResult<BlockedAttemptLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBlockedAttempts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _logService.GetLogsAsync(page, pageSize);
        return Ok(result);
    }
}
