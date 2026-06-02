namespace BatoClinic.Api.DTOs.Services;

// Used when admin creates a new clinic service/treatment.
public class CreateClinicServiceDto
{
    public Guid ServiceCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }
}