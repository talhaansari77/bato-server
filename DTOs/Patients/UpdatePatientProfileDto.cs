namespace BatoClinic.Api.DTOs.Patients;

// Used when patient updates own profile.
// Nullable fields allow partial update.
public class UpdatePatientProfileDto
{
    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public string? MedicalNotes { get; set; }
}