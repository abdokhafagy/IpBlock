using System.Collections.Concurrent;
using IPBlocker.Application.Interfaces;
using IPBlocker.Domain.Entities;

namespace IPBlocker.Infrastructure.Repositories;

public class InMemoryCountryRepository : ICountryRepository
{
    private readonly ConcurrentDictionary<string, Country> _countries = new(StringComparer.OrdinalIgnoreCase);

    public Task<bool> AddAsync(Country country)
    {
        var added = _countries.TryAdd(country.CountryCode, country);
        return Task.FromResult(added);
    }

    public Task<bool> RemoveAsync(string countryCode)
    {
        var removed = _countries.TryRemove(countryCode, out _);
        return Task.FromResult(removed);
    }

    public Task<Country?> GetByCodeAsync(string countryCode)
    {
        _countries.TryGetValue(countryCode, out var country);
        return Task.FromResult(country);
    }

    public Task<(IReadOnlyList<Country> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? search)
    {
        var query = _countries.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.CountryCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.CountryName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var all = query.OrderBy(c => c.CountryCode).ToList();
        var totalCount = all.Count;
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult<(IReadOnlyList<Country>, int)>((items, totalCount));
    }

    public Task<bool> ExistsAsync(string countryCode)
    {
        return Task.FromResult(_countries.ContainsKey(countryCode));
    }
}
