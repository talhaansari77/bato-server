namespace BatoClinic.Api.DTOs.Auth;

// Returned when the mobile app asks: "Who is currently logged in?"
public class MeResponseDto
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; }
}