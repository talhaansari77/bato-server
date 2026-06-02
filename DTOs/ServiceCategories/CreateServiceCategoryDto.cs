namespace BatoClinic.Api.DTOs.ServiceCategories;

// Used when admin creates a service category.
// Example categories: Hair Treatments, Skin Treatments, Face Treatments.
public class CreateServiceCategoryDto
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
}