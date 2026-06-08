using System.ComponentModel.DataAnnotations;

namespace BatoClinic.Api.Entities;

// DoctorProfile stores doctor-specific information.
// Login/account data stays in ApplicationUser.
// Medical/professional details stay here.
public class DoctorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // This links doctor profile to ASP.NET Identity user.
    public string UserId { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Specialization { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? LicenseNumber { get; set; }

    public int ExperienceYears { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public decimal ConsultationFee { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property.
    // This lets EF Core load the related Identity user.
    public ApplicationUser? User { get; set; }
    public ICollection<DoctorBranch> DoctorBranches { get; set; } = new List<DoctorBranch>();
    public ICollection<DoctorService> DoctorServices { get; set; } = new List<DoctorService>();
}