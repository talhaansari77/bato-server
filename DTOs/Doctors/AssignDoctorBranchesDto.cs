namespace BatoClinic.Api.DTOs.Doctors;

// Request body for assigning a doctor to one or more branches.
public class AssignDoctorBranchesDto
{
    public List<Guid> BranchIds { get; set; } = new();
}