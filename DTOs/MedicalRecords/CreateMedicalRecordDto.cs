namespace BatoClinic.Api.DTOs.MedicalRecords;

// Request body when doctor/admin creates a patient medical record.
public class CreateMedicalRecordDto
{
    public Guid PatientProfileId { get; set; }

    public Guid? AppointmentId { get; set; }

    public string Diagnosis { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string? Prescription { get; set; }

    public string? FollowUpInstructions { get; set; }

    public DateTime? RecordDate { get; set; }
}