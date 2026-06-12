namespace BatoClinic.Api.DTOs.TreatmentSessions;

// Request body when doctor/admin adds a session to a treatment plan.
public class CreateTreatmentSessionDto
{
    public Guid? AppointmentId { get; set; }

    public int SessionNumber { get; set; }

    public string Status { get; set; } = "Scheduled";

    public string? Notes { get; set; }

    public DateTime? ScheduledDate { get; set; }
}