namespace BatoClinic.Api.DTOs.Auth;

// Data sent from mobile app when a user registers.
public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Password { get; set; } = string.Empty;

    // For MVP, allowed values are: Patient, Doctor, Admin.
    // Later, Nurse can be added.
    public string Role { get; set; } = "Patient";
}