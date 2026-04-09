using IPBlocker.Application.DTOs;

namespace IPBlocker.Application.Interfaces;

public interface ILogService
{
    Task<PaginatedResult<BlockedAttemptLogResponse>> GetLogsAsync(int page, int pageSize);
}
