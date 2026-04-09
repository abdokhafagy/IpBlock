using IPBlocker.Application.DTOs;
using IPBlocker.Application.Exceptions;
using IPBlocker.Application.Interfaces;
using IPBlocker.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IPBlocker.Application.Services;

public class TemporaryBlockService : ITemporaryBlockService
{
    private readonly ITemporaryBlockRepository _repository;
    private readonly ILogger<TemporaryBlockService> _logger;

    public TemporaryBlockService(ITemporaryBlockRepository repository, ILogger<TemporaryBlockService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TemporalBlockResponse> AddTemporaryBlockAsync(TemporalBlockRequest request)
    {
        var exists = await _repository.ExistsAsync(request.CountryCode.ToUpperInvariant());
        if (exists)
        {
            _logger.LogWarning("Duplicate temporary block attempt for country: {CountryCode}", request.CountryCode);
            throw new DuplicateException($"Country '{request.CountryCode}' already has an active temporary block.");
        }

        var block = new TemporaryBlock
        {
            CountryCode = request.CountryCode.ToUpperInvariant(),
            DurationMinutes = request.DurationMinutes,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(request.DurationMinutes)
        };

        await _repository.AddAsync(block);
        _logger.LogInformation("Temporary block added: {CountryCode} for {Duration} min (expires {ExpiresAt})",
            block.CountryCode, block.DurationMinutes, block.ExpiresAt);

        return new TemporalBlockResponse
        {
            CountryCode = block.CountryCode,
            CreatedAt = block.CreatedAt,
            ExpiresAt = block.ExpiresAt,
            DurationMinutes = block.DurationMinutes
        };
    }
}
