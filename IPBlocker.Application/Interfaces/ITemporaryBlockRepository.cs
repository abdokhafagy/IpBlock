using IPBlocker.Domain.Entities;

namespace IPBlocker.Application.Interfaces;

public interface ITemporaryBlockRepository
{
    Task<bool> AddAsync(TemporaryBlock block);
    Task<bool> ExistsAsync(string countryCode);
    Task<bool> IsCountryTemporarilyBlockedAsync(string countryCode);
    Task<int> RemoveExpiredAsync();
    Task<IReadOnlyList<TemporaryBlock>> GetAllAsync();
}
