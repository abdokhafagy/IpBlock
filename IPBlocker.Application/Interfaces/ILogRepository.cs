using IPBlocker.Domain.Entities;

namespace IPBlocker.Application.Interfaces;

public interface ILogRepository
{
    Task AddAsync(BlockedAttemptLog log);
    Task<(IReadOnlyList<BlockedAttemptLog> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
}
