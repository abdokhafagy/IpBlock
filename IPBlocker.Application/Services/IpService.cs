using System.Net;
using IPBlocker.Application.DTOs;
using IPBlocker.Application.Exceptions;
using IPBlocker.Application.Interfaces;
using IPBlocker.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IPBlocker.Application.Services;

public class IpService : IIpService
{
    private readonly IIpGeolocationService _geolocationService;
    private readonly ICountryRepository _countryRepository;
    private readonly ITemporaryBlockRepository _temporaryBlockRepository;
    private readonly ILogRepository _logRepository;
    private readonly ILogger<IpService> _logger;

    public IpService(
        IIpGeolocationService geolocationService,
        ICountryRepository countryRepository,
        ITemporaryBlockRepository temporaryBlockRepository,
        ILogRepository logRepository,
        ILogger<IpService> logger)
    {
        _geolocationService = geolocationService;
        _countryRepository = countryRepository;
        _temporaryBlockRepository = temporaryBlockRepository;
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task<IpLookupResponse> LookupIpAsync(string? ipAddress, string callerIp)
    {
        var ip = string.IsNullOrWhiteSpace(ipAddress) ? callerIp : ipAddress;

        if (!IsValidIp(ip))
        {
            throw new Exceptions.ValidationException($"Invalid IP address format: '{ip}'.");
        }

        _logger.LogInformation("Looking up IP: {IpAddress}", ip);

        var result = await _geolocationService.LookupAsync(ip);
        return result;
    }

    public async Task<IpBlockCheckResponse> CheckBlockAsync(string callerIp, string userAgent)
    {
        _logger.LogInformation("Checking block status for caller IP: {CallerIp}", callerIp);

        var lookup = await _geolocationService.LookupAsync(callerIp);

        var isPermanentlyBlocked = await _countryRepository.ExistsAsync(lookup.CountryCode);
        var isTemporarilyBlocked = await _temporaryBlockRepository.IsCountryTemporarilyBlockedAsync(lookup.CountryCode);
        var isBlocked = isPermanentlyBlocked || isTemporarilyBlocked;

        // Log the attempt
        var log = new BlockedAttemptLog
        {
            IpAddress = callerIp,
            CountryCode = lookup.CountryCode,
            CountryName = lookup.CountryName,
            IsBlocked = isBlocked,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        await _logRepository.AddAsync(log);
        _logger.LogInformation("Block check for {CallerIp}: Country={CountryCode}, Blocked={IsBlocked}", callerIp, lookup.CountryCode, isBlocked);

        return new IpBlockCheckResponse
        {
            IpAddress = callerIp,
            CountryCode = lookup.CountryCode,
            CountryName = lookup.CountryName,
            IsBlocked = isBlocked
        };
    }

    private static bool IsValidIp(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out _);
    }
}
