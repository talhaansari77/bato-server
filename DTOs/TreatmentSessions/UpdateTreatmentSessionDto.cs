namespace BatoClinic.Api.DTOs.TreatmentSessions;

// Nullable fields allow partial updates.
public class UpdateTreatmentSessionDto
{
    public Guid? AppointmentId { get; set; }

    public int? SessionNumber { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? ScheduledDate { get; set; }
}