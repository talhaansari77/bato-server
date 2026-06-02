namespace BatoClinic.Api.DTOs.Auth;

// Data sent from mobile app when a user logs in.
public class LoginDto
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}