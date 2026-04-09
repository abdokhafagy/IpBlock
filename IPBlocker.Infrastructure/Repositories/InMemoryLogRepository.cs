using System.Collections.Concurrent;
using IPBlocker.Application.Interfaces;
using IPBlocker.Domain.Entities;

namespace IPBlocker.Infrastructure.Repositories;

public class InMemoryLogRepository : ILogRepository
{
    private readonly ConcurrentBag<BlockedAttemptLog> _logs = [];

    public Task AddAsync(BlockedAttemptLog log)
    {
        _logs.Add(log);
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<BlockedAttemptLog> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
    {
        var all = _logs.OrderByDescending(l => l.Timestamp).ToList();
        var totalCount = all.Count;
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult<(IReadOnlyList<BlockedAttemptLog>, int)>((items, totalCount));
    }
}
