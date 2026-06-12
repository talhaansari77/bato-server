namespace BatoClinic.Api.DTOs.ProgressPhotos;

// Clean response shape for progress photos.
public class ProgressPhotoResponseDto
{
    public Guid Id { get; set; }

    public Guid PatientProfileId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public Guid? TreatmentPlanId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string PhotoType { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime TakenAt { get; set; }

    public DateTime CreatedAt { get; set; }
}