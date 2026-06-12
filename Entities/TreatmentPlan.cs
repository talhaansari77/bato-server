using System.ComponentModel.DataAnnotations;

namespace BatoClinic.Api.Entities;

// TreatmentPlan represents a personalized care plan created by a doctor.
// Example: 6-session skin rejuvenation plan or hair nourishment plan.
public class TreatmentPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PatientProfileId { get; set; }

    public Guid DoctorProfileId { get; set; }

    public Guid? AppointmentId { get; set; }

    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PatientProfile? PatientProfile { get; set; }

    public DoctorProfile? DoctorProfile { get; set; }

    public Appointment? Appointment { get; set; }
    public ICollection<TreatmentSession> Sessions { get; set; } = new List<TreatmentSession>();

}