using System.ComponentModel.DataAnnotations;

namespace BatoClinic.Api.Entities;

// ProgressPhoto stores before/after or treatment progress image URLs.
// Actual file upload/storage will come later.
public class ProgressPhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PatientProfileId { get; set; }

    public Guid? TreatmentPlanId { get; set; }

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PhotoType { get; set; } = "Progress";

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime TakenAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PatientProfile? PatientProfile { get; set; }

    public TreatmentPlan? TreatmentPlan { get; set; }
}