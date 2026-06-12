using System.ComponentModel.DataAnnotations;

namespace BatoClinic.Api.Entities;

// MedicalRecord stores consultation and medical notes for a patient.
// Doctor/Admin can create records. Patient can view their own records.
public class MedicalRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PatientProfileId { get; set; }

    public Guid DoctorProfileId { get; set; }

    public Guid? AppointmentId { get; set; }

    [MaxLength(500)]
    public string Diagnosis { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Prescription { get; set; }

    [MaxLength(1000)]
    public string? FollowUpInstructions { get; set; }

    public DateTime RecordDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PatientProfile? PatientProfile { get; set; }

    public DoctorProfile? DoctorProfile { get; set; }

    public Appointment? Appointment { get; set; }
}