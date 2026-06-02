namespace BatoClinic.Api.DTOs.Branches;

// This is the shape of branch data returned by the API.
// It keeps responses clean and predictable for the mobile app.
public class BranchResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Address { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? Country { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}