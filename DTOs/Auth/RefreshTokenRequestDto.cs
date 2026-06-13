namespace BatoClinic.Api.DTOs.Auth;

// Request body for getting a new access token.
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}