using IPBlocker.Application.DTOs;
using IPBlocker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IPBlocke.Api.Controllers;

[ApiController]
[Route("api/countries")]
[Produces("application/json")]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly ITemporaryBlockService _temporaryBlockService;

    public CountriesController(ICountryService countryService, ITemporaryBlockService temporaryBlockService)
    {
        _countryService = countryService;
        _temporaryBlockService = temporaryBlockService;
    }

    /// <summary>
    /// Add a country to the permanent blocked list.
    /// </summary>
    [HttpPost("block")]
    [ProducesResponseType(typeof(BlockedCountryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BlockCountry([FromBody] BlockCountryRequest request)
    {
        var result = await _countryService.BlockCountryAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Remove a country from the permanent blocked list.
    /// </summary>
    [HttpDelete("block/{countryCode}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnblockCountry(string countryCode)
    {
        await _countryService.UnblockCountryAsync(countryCode);
        return NoContent();
    }

    /// <summary>
    /// Get a paginated list of blocked countries. Supports search by code or name.
    /// </summary>
    [HttpGet("blocked")]
    [ProducesResponseType(typeof(PaginatedResult<BlockedCountryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBlockedCountries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _countryService.GetBlockedCountriesAsync(page, pageSize, search);
        return Ok(result);
    }

    /// <summary>
    /// Add a temporary block for a country (auto-expires after durationMinutes).
    /// </summary>
    [HttpPost("temporal-block")]
    [ProducesResponseType(typeof(TemporalBlockResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddTemporalBlock([FromBody] TemporalBlockRequest request)
    {
        var result = await _temporaryBlockService.AddTemporaryBlockAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
