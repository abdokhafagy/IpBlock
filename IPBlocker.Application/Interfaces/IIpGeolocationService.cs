using IPBlocker.Application.DTOs;

namespace IPBlocker.Application.Interfaces;

public interface IIpGeolocationService
{
    Task<IpLookupResponse> LookupAsync(string ipAddress);
}
