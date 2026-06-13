using System.ComponentModel.DataAnnotations;

namespace BatoClinic.Api.Entities;

// RefreshToken stores long-lived login tokens.
// The mobile app uses refresh tokens to get new access tokens without logging in again.
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}