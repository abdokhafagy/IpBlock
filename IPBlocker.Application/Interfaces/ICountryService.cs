using IPBlocker.Application.DTOs;

namespace IPBlocker.Application.Interfaces;

public interface ICountryService
{
    Task<BlockedCountryResponse> BlockCountryAsync(BlockCountryRequest request);
    Task<bool> UnblockCountryAsync(string countryCode);
    Task<PaginatedResult<BlockedCountryResponse>> GetBlockedCountriesAsync(int page, int pageSize, string? search);
}
