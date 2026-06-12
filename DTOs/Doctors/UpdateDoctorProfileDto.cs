namespace BatoClinic.Api.DTOs.Doctors;

// Used when a doctor updates their own professional profile,
// or when admin updates a doctor profile.
public class UpdateDoctorProfileDto
{
    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Specialization { get; set; }

    public string? LicenseNumber { get; set; }

    public int? ExperienceYears { get; set; }

    public string? Bio { get; set; }

    public decimal? ConsultationFee { get; set; }

    public bool? IsAvailable { get; set; }
}