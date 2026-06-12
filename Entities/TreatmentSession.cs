using System.ComponentModel.DataAnnotations;

namespace BatoClinic.Api.Entities;

// TreatmentSession represents one session inside a treatment plan.
// Example: Session 1, Session 2, follow-up session, progress check, etc.
public class TreatmentSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TreatmentPlanId { get; set; }

    public Guid? AppointmentId { get; set; }

    public int SessionNumber { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled";

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TreatmentPlan? TreatmentPlan { get; set; }

    public Appointment? Appointment { get; set; }

}