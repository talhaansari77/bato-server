namespace BatoClinic.Api.DTOs.Doctors;

// Request body for assigning a doctor to one or more services.
public class AssignDoctorServicesDto
{
    public List<Guid> ServiceIds { get; set; } = new();
}