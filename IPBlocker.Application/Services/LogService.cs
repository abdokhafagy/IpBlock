using IPBlocker.Application.DTOs;
using IPBlocker.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace IPBlocker.Application.Services;

public class LogService : ILogService
{
    private readonly ILogRepository _logRepository;
    private readonly ILogger<LogService> _logger;

    public LogService(ILogRepository logRepository, ILogger<LogService> logger)
    {
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task<PaginatedResult<BlockedAttemptLogResponse>> GetLogsAsync(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var (items, total) = await _logRepository.GetAllAsync(page, pageSize);

        return new PaginatedResult<BlockedAttemptLogResponse>
        {
            Items = items.Select(l => new BlockedAttemptLogResponse
            {
                IpAddress = l.IpAddress,
                Timestamp = l.Timestamp,
                CountryCode = l.CountryCode,
                CountryName = l.CountryName,
                IsBlocked = l.IsBlocked,
                UserAgent = l.UserAgent
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
