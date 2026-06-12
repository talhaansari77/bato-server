namespace BatoClinic.Api.DTOs.TreatmentSessions;

// Clean response shape for treatment sessions.
public class TreatmentSessionResponseDto
{
    public Guid Id { get; set; }

    public Guid TreatmentPlanId { get; set; }

    public Guid? AppointmentId { get; set; }

    public int SessionNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public DateTime CreatedAt { get; set; }
}