namespace BatoClinic.Api.Entities;

// ServiceCategory groups services like Hair, Skin, and Face treatments.
public class ServiceCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property:
    // One category can have many services.
    public ICollection<ClinicService> Services { get; set; } = new List<ClinicService>();
}