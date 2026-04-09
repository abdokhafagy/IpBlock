using System.Collections.Concurrent;
using IPBlocker.Application.Interfaces;
using IPBlocker.Domain.Entities;

namespace IPBlocker.Infrastructure.Repositories;

public class InMemoryTemporaryBlockRepository : ITemporaryBlockRepository
{
    private readonly ConcurrentDictionary<string, TemporaryBlock> _blocks = new(StringComparer.OrdinalIgnoreCase);

    public Task<bool> AddAsync(TemporaryBlock block)
    {
        var added = _blocks.TryAdd(block.CountryCode, block);
        return Task.FromResult(added);
    }

    public Task<bool> ExistsAsync(string countryCode)
    {
        if (_blocks.TryGetValue(countryCode, out var block))
        {
            // If expired, remove it and report as not existing
            if (block.IsExpired)
            {
                _blocks.TryRemove(countryCode, out _);
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> IsCountryTemporarilyBlockedAsync(string countryCode)
    {
        return ExistsAsync(countryCode);
    }

    public Task<int> RemoveExpiredAsync()
    {
        var expiredKeys = _blocks
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        var removedCount = 0;
        foreach (var key in expiredKeys)
        {
            if (_blocks.TryRemove(key, out _))
                removedCount++;
        }

        return Task.FromResult(removedCount);
    }

    public Task<IReadOnlyList<TemporaryBlock>> GetAllAsync()
    {
        var all = _blocks.Values
            .Where(b => !b.IsExpired)
            .OrderBy(b => b.CountryCode)
            .ToList();

        return Task.FromResult<IReadOnlyList<TemporaryBlock>>(all);
    }
}
