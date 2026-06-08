namespace BatoClinic.Api.Entities;

// Join table between DoctorProfile and Branch.
// One doctor can work in many branches.
// One branch can have many doctors.
public class DoctorBranch
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DoctorProfileId { get; set; }

    public Guid BranchId { get; set; }

    public DoctorProfile? DoctorProfile { get; set; }

    public Branch? Branch { get; set; }
}