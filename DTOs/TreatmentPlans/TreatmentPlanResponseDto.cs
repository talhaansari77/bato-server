namespace BatoClinic.Api.DTOs.TreatmentPlans;

// Clean response shape for treatment plans.
public class TreatmentPlanResponseDto
{
    public Guid Id { get; set; }

    public Guid PatientProfileId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public Guid DoctorProfileId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public Guid? AppointmentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}