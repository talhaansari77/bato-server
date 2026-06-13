namespace BatoClinic.Api.DTOs.Auth;

// Request body for logout.
// We revoke the refresh token so it cannot be used again.
public class LogoutDto
{
    public string RefreshToken { get; set; } = string.Empty;
}