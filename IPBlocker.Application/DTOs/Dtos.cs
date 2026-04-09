using System.ComponentModel.DataAnnotations;

namespace IPBlocker.Application.DTOs;

// ────────────────────────────────────────
// Country DTOs
// ────────────────────────────────────────

public class BlockCountryRequest
{
    [Required(ErrorMessage = "Country code is required.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters (ISO 3166-1 alpha-2).")]
    [RegularExpression("^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters.")]
    public string CountryCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Country name must be between 2 and 100 characters.")]
    public string CountryName { get; set; } = string.Empty;
}

public class BlockedCountryResponse
{
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ────────────────────────────────────────
// Temporal Block DTOs
// ────────────────────────────────────────

public class TemporalBlockRequest
{
    [Required(ErrorMessage = "Country code is required.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters.")]
    [RegularExpression("^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters.")]
    public string CountryCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Duration in minutes is required.")]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes (24 hours).")]
    public int DurationMinutes { get; set; }
}

public class TemporalBlockResponse
{
    public string CountryCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int DurationMinutes { get; set; }
}

// ────────────────────────────────────────
// IP Lookup DTOs
// ────────────────────────────────────────

public class IpLookupResponse
{
    public string IpAddress { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string Isp { get; set; } = string.Empty;
}

public class IpBlockCheckResponse
{
    public string IpAddress { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
}

// ────────────────────────────────────────
// Log DTOs
// ────────────────────────────────────────

public class BlockedAttemptLogResponse
{
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public string UserAgent { get; set; } = string.Empty;
}

// ────────────────────────────────────────
// Pagination
// ────────────────────────────────────────

public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

// ────────────────────────────────────────
// Error Response
// ────────────────────────────────────────

public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}
