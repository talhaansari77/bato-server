using System.ComponentModel.DataAnnotations;

namespace BatoClinic.Api.Entities;

// PatientProfile stores patient-specific information.
// Login/account data stays in ApplicationUser.
// Medical and personal profile details live here.
public class PatientProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Links this patient profile to ASP.NET Identity user.
    public string UserId { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(30)]
    public string? Gender { get; set; }

    [MaxLength(150)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(30)]
    public string? EmergencyContactPhone { get; set; }

    [MaxLength(1000)]
    public string? MedicalNotes { get; set; }

    public bool VipStatus { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property to Identity user.
    public ApplicationUser? User { get; set; }
}