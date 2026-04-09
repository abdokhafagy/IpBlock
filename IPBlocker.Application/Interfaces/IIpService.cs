using IPBlocker.Application.DTOs;

namespace IPBlocker.Application.Interfaces;

public interface IIpService
{
    Task<IpLookupResponse> LookupIpAsync(string? ipAddress, string callerIp);
    Task<IpBlockCheckResponse> CheckBlockAsync(string callerIp, string userAgent);
}
