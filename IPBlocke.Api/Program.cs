using IPBlocke.Api.Extensions;
using IPBlocke.Api.Middleware;
using Microsoft.OpenApi.Models;

namespace IPBlocke.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ─── Controllers & JSON ──────────────────────────────────────
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                // Keep default model-state validation (returns 400 with details)
            });

        // ─── Swagger / OpenAPI ───────────────────────────────────────
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "IP Blocker API",
                Version = "v1",
                Description = "API for managing blocked countries and validating IP addresses using geolocation.",
                Contact = new OpenApiContact
                {
                    Name = "IPBlocker Team"
                }
            });
        });

        // ─── Application & Infrastructure DI ─────────────────────────
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // ─── Logging ─────────────────────────────────────────────────
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        var app = builder.Build();

        // ─── Middleware pipeline ─────────────────────────────────────
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Swagger always enabled (for demo purposes)
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "IP Blocker API v1");
        });

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
