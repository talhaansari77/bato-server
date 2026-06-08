namespace BatoClinic.Api.DTOs.Doctors;

// Clean response shape for doctor data.
// We do not expose internal Identity fields.
public class DoctorResponseDto
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string Specialization { get; set; } = string.Empty;

    public string? LicenseNumber { get; set; }

    public int ExperienceYears { get; set; }

    public string? Bio { get; set; }

    public decimal ConsultationFee { get; set; }

    public bool IsAvailable { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; }
}