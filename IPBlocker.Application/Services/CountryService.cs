using IPBlocker.Application.DTOs;
using IPBlocker.Application.Exceptions;
using IPBlocker.Application.Interfaces;
using IPBlocker.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IPBlocker.Application.Services;

public class CountryService : ICountryService
{
    private readonly ICountryRepository _countryRepository;
    private readonly ILogger<CountryService> _logger;

    public CountryService(ICountryRepository countryRepository, ILogger<CountryService> logger)
    {
        _countryRepository = countryRepository;
        _logger = logger;
    }

    public async Task<BlockedCountryResponse> BlockCountryAsync(BlockCountryRequest request)
    {
        var exists = await _countryRepository.ExistsAsync(request.CountryCode);
        if (exists)
        {
            _logger.LogWarning("Attempt to block already-blocked country: {CountryCode}", request.CountryCode);
            throw new DuplicateException($"Country '{request.CountryCode}' is already blocked.");
        }

        var country = new Country
        {
            CountryCode = request.CountryCode.ToUpperInvariant(),
            CountryName = request.CountryName,
            CreatedAt = DateTime.UtcNow
        };

        await _countryRepository.AddAsync(country);
        _logger.LogInformation("Country blocked: {CountryCode} - {CountryName}", country.CountryCode, country.CountryName);

        return new BlockedCountryResponse
        {
            CountryCode = country.CountryCode,
            CountryName = country.CountryName,
            CreatedAt = country.CreatedAt
        };
    }

    public async Task<bool> UnblockCountryAsync(string countryCode)
    {
        var removed = await _countryRepository.RemoveAsync(countryCode.ToUpperInvariant());
        if (!removed)
        {
            _logger.LogWarning("Attempt to unblock non-existent country: {CountryCode}", countryCode);
            throw new NotFoundException($"Country '{countryCode}' is not in the blocked list.");
        }

        _logger.LogInformation("Country unblocked: {CountryCode}", countryCode);
        return true;
    }

    public async Task<PaginatedResult<BlockedCountryResponse>> GetBlockedCountriesAsync(int page, int pageSize, string? search)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var (items, total) = await _countryRepository.GetAllAsync(page, pageSize, search);

        return new PaginatedResult<BlockedCountryResponse>
        {
            Items = items.Select(c => new BlockedCountryResponse
            {
                CountryCode = c.CountryCode,
                CountryName = c.CountryName,
                CreatedAt = c.CreatedAt
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
