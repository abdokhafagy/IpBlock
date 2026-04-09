using IPBlocker.Application.Interfaces;
using IPBlocker.Application.Services;
using IPBlocker.Infrastructure.Repositories;
using IPBlocker.Infrastructure.External;
using IPBlocker.Infrastructure.BackgroundServices;

namespace IPBlocke.Api.Extensions;

/// <summary>
/// Extension methods for registering application services with the DI container.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application-layer services
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IIpService, IpService>();
        services.AddScoped<ITemporaryBlockService, TemporaryBlockService>();
        services.AddScoped<ILogService, LogService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Repositories — singletons because they hold in-memory state
        services.AddSingleton<ICountryRepository, InMemoryCountryRepository>();
        services.AddSingleton<ITemporaryBlockRepository, InMemoryTemporaryBlockRepository>();
        services.AddSingleton<ILogRepository, InMemoryLogRepository>();

        // HttpClient for ipapi.co with sensible defaults
        services.AddHttpClient<IIpGeolocationService, IpApiGeolocationService>(client =>
        {
            client.BaseAddress = new Uri("https://ipapi.co/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "IPBlocker-API/1.0");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Background cleanup service
        services.AddHostedService<TemporaryBlockCleanupService>();

        return services;
    }
}
