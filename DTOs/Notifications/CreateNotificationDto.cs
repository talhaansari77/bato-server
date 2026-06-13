namespace BatoClinic.Api.DTOs.Notifications;

// Request body when admin creates a notification for a user.
public class CreateNotificationDto
{
    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = "General";
}