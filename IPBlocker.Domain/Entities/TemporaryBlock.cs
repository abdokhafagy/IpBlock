namespace IPBlocker.Domain.Entities;

public class TemporaryBlock
{
    public string CountryCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public int DurationMinutes { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
