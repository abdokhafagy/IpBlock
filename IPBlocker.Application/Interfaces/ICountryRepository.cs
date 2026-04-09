using IPBlocker.Domain.Entities;

namespace IPBlocker.Application.Interfaces;

public interface ICountryRepository
{
    Task<bool> AddAsync(Country country);
    Task<bool> RemoveAsync(string countryCode);
    Task<Country?> GetByCodeAsync(string countryCode);
    Task<(IReadOnlyList<Country> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search);
    Task<bool> ExistsAsync(string countryCode);
}
