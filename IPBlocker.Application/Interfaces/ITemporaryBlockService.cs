using IPBlocker.Application.DTOs;

namespace IPBlocker.Application.Interfaces;

public interface ITemporaryBlockService
{
    Task<TemporalBlockResponse> AddTemporaryBlockAsync(TemporalBlockRequest request);
}
