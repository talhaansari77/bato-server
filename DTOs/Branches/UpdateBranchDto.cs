namespace BatoClinic.Api.DTOs.Branches;

// This DTO is used when updating a branch.
// Nullable properties mean the admin can update only the fields they want.
public class UpdateBranchDto
{
    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public bool? IsActive { get; set; }
}