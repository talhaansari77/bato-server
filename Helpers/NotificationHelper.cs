using BatoClinic.Api.Data;
using BatoClinic.Api.Entities;

namespace BatoClinic.Api.Helpers;

// NotificationHelper centralizes notification creation.
// Instead of repeating notification creation code in every controller,
// we call this helper whenever something important happens.
public static class NotificationHelper
{
    public static async Task CreateNotificationAsync(
        AppDbContext context,
        string userId,
        string title,
        string message,
        string type = "General")
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }
}