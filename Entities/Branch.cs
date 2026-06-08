namespace BatoClinic.Api.Entities;

// Branch represents one clinic location.
// BATO supports multiple branches from the beginning.
public class Branch
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Address { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? Country { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DoctorBranch> DoctorBranches { get; set; } = new List<DoctorBranch>();
}