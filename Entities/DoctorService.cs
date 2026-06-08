namespace BatoClinic.Api.Entities;

// Join table between DoctorProfile and ClinicService.
// One doctor can perform many services.
// One service can be performed by many doctors.
public class DoctorService
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DoctorProfileId { get; set; }

    public Guid ClinicServiceId { get; set; }

    public DoctorProfile? DoctorProfile { get; set; }

    public ClinicService? ClinicService { get; set; }
}