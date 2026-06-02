namespace BatoClinic.Api.DTOs.Services;

// Clean response shape for services/treatments.
public class ClinicServiceResponseDto
{
    public Guid Id { get; set; }

    public Guid ServiceCategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}