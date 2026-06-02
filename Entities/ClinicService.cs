namespace BatoClinic.Api.Entities;

// ClinicService represents one treatment/service in BATO.
// Example: HydraFacial Treatment, Hair Nourishment, Botox Consultation.
public class ClinicService
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ServiceCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property:
    // Each service belongs to one category.
    public ServiceCategory? ServiceCategory { get; set; }
}