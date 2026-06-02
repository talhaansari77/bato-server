namespace BatoClinic.Api.DTOs.Branches;

// DTO = Data Transfer Object.
// This is the shape of data the client sends when creating a branch.
// We use DTOs so the API does not accept database-only fields like Id or CreatedAt.
public class CreateBranchDto
{
    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Address { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? Country { get; set; }
}