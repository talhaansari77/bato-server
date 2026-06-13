using System.Security.Claims;
using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.Notifications;
using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    // POST /api/notifications
    // Admin creates an in-app notification for a user.
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<NotificationResponseDto>> CreateNotification(CreateNotificationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId))
        {
            return BadRequest(new { message = "User id is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Notification title is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            return BadRequest(new { message = "Notification message is required" });
        }

        var userExists = await _context.Users.AnyAsync(user => user.Id == dto.UserId && user.IsActive);

        if (!userExists)
        {
            return BadRequest(new { message = "Valid active user is required" });
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Title = dto.Title.Trim(),
            Message = dto.Message.Trim(),
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "General" : dto.Type.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return Ok(ToNotificationResponse(notification));
    }

    // GET /api/notifications/my
    // User views their own notifications.
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetMyNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var notifications = await _context.Notifications
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Select(notification => ToNotificationResponse(notification))
            .ToListAsync();

        return Ok(notifications);
    }

    // PATCH /api/notifications/{id}/read
    // User marks one own notification as read.
    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult<NotificationResponseDto>> MarkAsRead(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (notification is null)
        {
            return NotFound(new { message = "Notification not found" });
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ToNotificationResponse(notification));
    }

    // PATCH /api/notifications/read-all
    // User marks all own notifications as read.
    [HttpPatch("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var notifications = await _context.Notifications
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "All notifications marked as read",
            updatedCount = notifications.Count
        });
    }

    // DELETE /api/notifications/{id}
    // User deletes one own notification.
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteNotification(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (notification is null)
        {
            return NotFound(new { message = "Notification not found" });
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Notification deleted successfully" });
    }

    private static NotificationResponseDto ToNotificationResponse(Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            CreatedAt = notification.CreatedAt
        };
    }
}