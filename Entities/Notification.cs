using System.ComponentModel.DataAnnotations;

namespace BatoClinic.Api.Entities;

// Notification stores in-app messages for users.
// Later we can connect this with Firebase push notifications.
public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Identity user who receives the notification.
    public string UserId { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Type { get; set; } = "General";

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}