using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using IPBlocker.Application.DTOs;
using IPBlocker.Application.Exceptions;
using IPBlocker.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IPBlocker.Infrastructure.External;

/// <summary>
/// Integrates with ipapi.co for IP geolocation.
/// Falls back gracefully on rate limits (HTTP 429) and errors.
/// </summary>
public class IpApiGeolocationService : IIpGeolocationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IpApiGeolocationService> _logger;
    private readonly string _apiKey;

    public IpApiGeolocationService(HttpClient httpClient, IConfiguration configuration, ILogger<IpApiGeolocationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["IpApi:ApiKey"] ?? string.Empty;
    }

    public async Task<IpLookupResponse> LookupAsync(string ipAddress)
    {
        try
        {
            // api.ipapi.com format: http://api.ipapi.com/api/{ip}?access_key={key}
            var url = string.IsNullOrEmpty(_apiKey)
                ? $"http://api.ipapi.com/api/{ipAddress}"
                : $"http://api.ipapi.com/api/{ipAddress}?access_key={_apiKey}";

            _logger.LogInformation("Calling api.ipapi.com for IP: {IpAddress}", ipAddress);

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Rate limited by api.ipapi.com for IP: {IpAddress}", ipAddress);
                throw new ExternalApiException("Geolocation API rate limit exceeded. Please try again later.");
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<IpApiResponse>(content);

            if (data is null || data.IsError)
            {
                var reason = data?.Reason ?? "Unknown error";
                _logger.LogWarning("api.ipapi.com returned error for {IpAddress}: {Reason}", ipAddress, reason);
                throw new ExternalApiException($"Geolocation lookup failed: {reason}");
            }

            return new IpLookupResponse
            {
                IpAddress = data.Ip ?? ipAddress,
                CountryCode = data.CountryCode ?? "Unknown",
                CountryName = data.CountryName ?? "Unknown",
                Isp = data.Org ?? "Unknown"
            };
        }
        catch (ExternalApiException)
        {
            throw; // Re-throw our own exceptions
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling ipapi.co for IP: {IpAddress}", ipAddress);
            throw new ExternalApiException("Failed to connect to geolocation API.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout calling ipapi.co for IP: {IpAddress}", ipAddress);
            throw new ExternalApiException("Geolocation API request timed out.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during geolocation lookup for IP: {IpAddress}", ipAddress);
            throw new ExternalApiException("An unexpected error occurred during geolocation lookup.", ex);
        }
    }

    // ── ipapi.com response model ──
    private class IpApiResponse
    {
        [JsonPropertyName("ip")]
        public string? Ip { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("country_name")]
        public string? CountryName { get; set; }

        [JsonPropertyName("connection")]
        public ConnectionInfo? Connection { get; set; }

        public string? Org => Connection?.Isp;

        [JsonPropertyName("error")]
        public JsonElement? ErrorInfo { get; set; }

        public bool IsError => ErrorInfo.HasValue && ErrorInfo.Value.ValueKind == JsonValueKind.Object;
        
        public string? Reason => IsError ? ErrorInfo!.Value.GetProperty("info").GetString() : null;
    }

    private class ConnectionInfo
    {
        [JsonPropertyName("isp")]
        public string? Isp { get; set; }
    }
}
