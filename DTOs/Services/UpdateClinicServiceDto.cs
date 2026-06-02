namespace BatoClinic.Api.DTOs.Services;

// Used when admin updates an existing service.
// Nullable fields allow partial updates.
public class UpdateClinicServiceDto
{
    public Guid? ServiceCategoryId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? DurationMinutes { get; set; }

    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }
}