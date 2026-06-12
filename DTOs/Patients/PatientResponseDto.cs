namespace BatoClinic.Api.DTOs.Patients;

// Clean response shape for patient profile data.
// We avoid returning Identity internals like PasswordHash.
public class PatientResponseDto
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public string? MedicalNotes { get; set; }

    public bool VipStatus { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}