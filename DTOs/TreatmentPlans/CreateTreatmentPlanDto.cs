namespace BatoClinic.Api.DTOs.TreatmentPlans;

// Request body when doctor/admin creates a treatment plan.
public class CreateTreatmentPlanDto
{
    public Guid PatientProfileId { get; set; }

    public Guid? AppointmentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}