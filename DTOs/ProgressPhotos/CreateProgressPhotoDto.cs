namespace BatoClinic.Api.DTOs.ProgressPhotos;

// Request body when patient uploads/adds a progress photo.
// For now ImageUrl is sent directly. Later we will support actual file upload.
public class CreateProgressPhotoDto
{
    public Guid? TreatmentPlanId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string PhotoType { get; set; } = "Progress";

    public string? Notes { get; set; }

    public DateTime? TakenAt { get; set; }
}