namespace BatoClinic.Api.DTOs.MedicalRecords;

// Nullable fields allow partial update.
public class UpdateMedicalRecordDto
{
    public string? Diagnosis { get; set; }

    public string? Notes { get; set; }

    public string? Prescription { get; set; }

    public string? FollowUpInstructions { get; set; }

    public DateTime? RecordDate { get; set; }
}