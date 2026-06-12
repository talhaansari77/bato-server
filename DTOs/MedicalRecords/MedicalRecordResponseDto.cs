namespace BatoClinic.Api.DTOs.MedicalRecords;

// Clean response shape for patient medical records.
public class MedicalRecordResponseDto
{
    public Guid Id { get; set; }

    public Guid PatientProfileId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public Guid DoctorProfileId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public Guid? AppointmentId { get; set; }

    public string Diagnosis { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string? Prescription { get; set; }

    public string? FollowUpInstructions { get; set; }

    public DateTime RecordDate { get; set; }

    public DateTime CreatedAt { get; set; }
}